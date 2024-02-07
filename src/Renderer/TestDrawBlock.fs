﻿module TestDrawBlock
open GenerateData
open Elmish


//-------------------------------------------------------------------------------------------//
//--------Types to represent tests with (possibly) random data, and results from tests-------//
//-------------------------------------------------------------------------------------------//
module TestLib =

    /// convenience unsafe function to extract Ok part of Result or fail if value is Error
    let getOkOrFail (res: Result<'a,string>) =
        match res with
        | Ok x -> x
        | Error mess ->
            failwithf "%s" mess


    type TestStatus =
            | Fail of string
            | Exception of string

    type Test<'a> = {
        Name: string
        Samples: Gen<'a>
        StartFrom: int
        /// The 1st argument is the test number: allows assertions that fail on a specific sample
        /// to display just one sample.
        /// The return value is None if test passes, or Some message if it fails.
        Assertion: int -> 'a -> string option
        }

    type TestResult<'a> = {
        TestName: string
        TestData: Gen<'a>
        FirstSampleTested: int
        TestErrors: (int * TestStatus) list
    }

    let catchException name func arg =
        try
            Ok (func arg)
        with
            | e ->
                Error ($"Exception when running {name}\n" + e.StackTrace)
            
    /// Run the Test samples from 0 up to test.Size - 1.
    /// The return list contains all failures or exceptions: empty list => everything has passed.
    /// This will always run to completion: use truncate if text.Samples.Size is too large.
    let runTests (test: Test<'a>) : TestResult<'a>  =
        [test.StartFrom..test.Samples.Size - 1]
        |> List.map (fun n ->
                catchException $"generating test {n} from {test.Name}" test.Samples.Data n
                |> (fun res -> n,res)
           )           
        |> List.collect (function
                            | n, Error mess -> [n, Exception mess]
                            | n, Ok sample ->
                                match catchException $"'test.Assertion' on test {n} from 'runTests'" (test.Assertion n) sample with
                                | Ok None -> []
                                | Ok (Some failure) -> [n,Fail failure]
                                | Error (mess) -> [n,Exception mess])
        |> (fun resL ->                
                {
                    TestName = test.Name
                    FirstSampleTested = test.StartFrom
                    TestData = test.Samples
                    TestErrors =  resL
                })
 
 
            
(******************************************************************************************
   This submodule contains a set of functions that enable random data generation
   for property-based testing of Draw Block wire routing functions.
   basic idea.
   1. Generate, in various ways, random circuit layouts
   2. For each layout apply smartautoroute to regenerate all wires
   3. Apply check functions to see if the resulting wire routing obeys "good layout" rules.
   4. Output any layouts with anomalous wire routing
*******************************************************************************************)
module HLPTick3 =
    open EEExtensions
    open Optics
    open Optics.Operators
    open DrawHelpers
    open Helpers
    open CommonTypes
    open ModelType
    open DrawModelType
    open Sheet.SheetInterface
    open GenerateData
    open TestLib

    /// create an initial empty Sheet Model 
    let initSheetModel = DiagramMainView.init().Sheet

    /// Optic to access SheetT.Model from Issie Model
    let sheetModel_ = sheet_

    /// Optic to access BusWireT.Model from SheetT.Model
    let busWireModel_ = SheetT.wire_

    /// Optic to access SymbolT.Model from SheetT.Model
    let symbolModel_ = SheetT.symbol_

    /// allowed max X or y coord of svg canvas
    let maxSheetCoord = Sheet.Constants.defaultCanvasSize
    let middleOfSheet = {X=maxSheetCoord/2.;Y=maxSheetCoord/2.}

    /// Used throughout to compare labels since these are case invariant "g1" = "G1"
    let caseInvariantEqual str1 str2 =
        String.toUpper str1 = String.toUpper str2
 

    /// Identify a port from its component label and number.
    /// Usually both an input and output port will mathc this, so
    /// the port is only unique if it is known to be input or output.
    /// used to specify the ends of wires, since tehee are known to be
    /// connected to outputs (source) or inputs (target).
    type SymbolPort = { Label: string; PortNumber: int }

    /// convenience function to make SymbolPorts
    let portOf (label:string) (number: int) =
        {Label=label; PortNumber = number}


    //-----------------------------------------------------------------------------------------------
    // visibleSegments is included here as ahelper for info, and because it is needed in project work
    //-----------------------------------------------------------------------------------------------

    /// The visible segments of a wire, as a list of vectors, from source end to target end.
    /// Note that in a wire with n segments a zero length (invisible) segment at any index [1..n-2] is allowed 
    /// which if present causes the two segments on either side of it to coalesce into a single visible segment.
    /// A wire can have any number of visible segments - even 1.
    let visibleSegments (wId: ConnectionId) (model: SheetT.Model): XYPos list =

        let wire = model.Wire.Wires[wId] // get wire from model

        /// helper to match even and off integers in patterns (active pattern)
        let (|IsEven|IsOdd|) (n: int) = match n % 2 with | 0 -> IsEven | _ -> IsOdd

        /// Convert seg into its XY Vector (from start to end of segment).
        /// index must be the index of seg in its containing wire.
        let getSegmentVector (index:int) (seg: BusWireT.Segment) =
            // The implicit horizontal or vertical direction  of a segment is determined by 
            // its index in the list of wire segments and the wire initial direction
            match index, wire.InitialOrientation with
            | IsEven, BusWireT.Vertical | IsOdd, BusWireT.Horizontal -> {X=0.; Y=seg.Length}
            | IsEven, BusWireT.Horizontal | IsOdd, BusWireT.Vertical -> {X=seg.Length; Y=0.}

        /// Return a list of segment vectors with 3 vectors coalesced into one visible equivalent
        /// if this is possible, otherwise return segVecs unchanged.
        /// Index must be in range 1..segVecs
        let tryCoalesceAboutIndex (segVecs: XYPos list) (index: int)  =
            if segVecs[index] =~ XYPos.zero
            then
                segVecs[0..index-2] @
                [segVecs[index-1] + segVecs[index+1]] @
                segVecs[index+2..segVecs.Length - 1]
            else
                segVecs

        wire.Segments
        |> List.mapi getSegmentVector
        |> (fun segVecs ->
                (segVecs,[1..segVecs.Length-2])
                ||> List.fold tryCoalesceAboutIndex)


//------------------------------------------------------------------------------------------------------------------------//
//------------------------------functions to build issue schematics programmatically--------------------------------------//
//------------------------------------------------------------------------------------------------------------------------//
    module Builder =

        /// Place a new symbol with label symLabel onto the Sheet with given position.
        /// Return error if symLabel is not unique on sheet, or if position is outside allowed sheet coordinates (0 - maxSheetCoord).
        /// To be safe place components close to (maxSheetCoord/2.0, maxSheetCoord/2.0).
        /// symLabel - the component label, will be uppercased to make a standard label name
        /// compType - the type of the component
        /// position - the top-left corner of the symbol outline.
        /// model - the Sheet model into which the new symbol is added.
        let placeSymbol (symLabel: string) (compType: ComponentType) (position: XYPos) (model: SheetT.Model) : Result<SheetT.Model, string> =
            let symLabel = String.toUpper symLabel // make label into its standard casing
            let symModel, symId = SymbolUpdate.addSymbol [] (model.Wire.Symbol) position compType symLabel
            let sym = symModel.Symbols[symId]
            match position + sym.getScaledDiagonal with
            | {X=x;Y=y} when x > maxSheetCoord || y > maxSheetCoord ->
                Error $"symbol '{symLabel}' position {position + sym.getScaledDiagonal} lies outside allowed coordinates"
            | _ ->
                model
                |> Optic.set symbolModel_ symModel
                |> SheetUpdate.updateBoundingBoxes // could optimise this by only updating symId bounding boxes
                |> Ok

        /// Place a new symbol onto the Sheet with given position and scaling (use default scale if this is not specified).
        /// The ports on the new symbol will be determined by the input and output components on some existing sheet in project.
        /// Return error if symLabel is not unique on sheet, or ccSheetName is not the name of some other sheet in project.
        let placeCustomSymbol
                (symLabel: string)
                (ccSheetName: string)
                (project: Project)
                (scale: XYPos)
                (position: XYPos)
                (model: SheetT.Model)
                    : Result<SheetT.Model, string> =
           let symbolMap = model.Wire.Symbol.Symbols
           if caseInvariantEqual ccSheetName project.OpenFileName then
                Error "Can't create custom component with name same as current opened sheet"        
            elif not <| List.exists (fun (ldc: LoadedComponent) -> caseInvariantEqual ldc.Name ccSheetName) project.LoadedComponents then
                Error "Can't create custom component unless a sheet already exists with smae name as ccSheetName"
            elif symbolMap |> Map.exists (fun _ sym ->  caseInvariantEqual sym.Component.Label symLabel) then
                Error "Can't create custom component with duplicate Label"
            else
                let canvas = model.GetCanvasState()
                let ccType: CustomComponentType =
                    {
                        Name = ccSheetName
                        InputLabels = Extractor.getOrderedCompLabels (Input1 (0, None)) canvas
                        OutputLabels = Extractor.getOrderedCompLabels (Output 0) canvas
                        Form = None
                        Description = None
                    }
                placeSymbol symLabel (Custom ccType) position model

        /// <summary>Helper function to get symbol ID from symbol label and symbol model.</summary>
        /// <param name="symLabel">Label of symbol to find.</param>
        /// <param name="symModel">Symbol model to search in.</param>
        /// <returns>ComponentId of symbol specified by symLabel.</returns>
        /// <remarks>(Not sure whether this is the correct place for this.)</remarks>
        let getSymId (symLabel: string) (symModel: SymbolT.Model): ComponentId = 
            mapValues symModel.Symbols
            |> Array.tryFind (fun sym -> caseInvariantEqual sym.Component.Label symLabel)
            |> function
                | Some x -> x.Id
                | _ -> failwithf "TestDrawBlock.rotateSymbol: symLabel (%A) not found" symLabel

        /// <summary>Helper function to re-route all wires on the sheet.</summary>
        /// <param name="sheetModel">Sheet model to reroute.</param>
        /// <returns>Updated sheet model.</returns>
        /// <remarks>(Not sure whether this is the correct place for this.)</remarks>
        let autoRouteAllWires (sheetModel: SheetT.Model): SheetT.Model =
            let wireModel = Optic.get busWireModel_ sheetModel
            let wireIds =
                wireModel.Wires
                |> Map.fold (fun keys wireId _ -> keys @ [wireId]) []
            let updateFolder prevWireModel wireId =
                let updatedWire = BusWireRoute.smartAutoroute prevWireModel prevWireModel.Wires[wireId]
                let updatedWires = Map.add wireId updatedWire prevWireModel.Wires
                {prevWireModel with Wires=updatedWires}
            let updatedWireModel = 
                (wireModel, wireIds)
                ||> List.fold updateFolder
            Optic.set busWireModel_ updatedWireModel sheetModel
        
        /// <summary>Rotate anti-clockwise a symbol on the sheet model by an arbitrary 
        /// degree (either 0, 90, 180, 270).</summary>
        /// <param name="symLabel">Label of symbol to rotate, different from ID.</param>
        /// <param name="rotate">Anti-clockwise degrees of rotation, of type Rotation.</param>
        /// <param name="model">Pre-rotation sheet model.</param>
        /// <returns>Updated sheet model with the rotation.</returns>
        /// <remarks>This is terrible code, but it is necessary as we need to compensate for 
        /// the fact that SymbolResizeHelpers.rotateSymbol does not work for 0 or 180 degrees.
        /// Recursion could be used here, but that would not make the code more readable.</remarks>
        let rotateSymbol (symLabel: string) (rotate: Rotation) (model: SheetT.Model) : (SheetT.Model) =  
            let rotateAntiClockwiseBy90 (model: SheetT.Model): SheetT.Model=
                let symModel = Optic.get symbolModel_ model
                let symId = getSymId symLabel symModel
                let updatedSymModel = 
                    SymbolUpdate.updateSymbol (SymbolResizeHelpers.rotateSymbol Degree90) (symId) (Optic.get symbolModel_ model)
                Optic.set symbolModel_ updatedSymModel model

            match rotate with 
            | Degree0 -> model
            | Degree90 -> model |> rotateAntiClockwiseBy90
            | Degree180 -> model |> rotateAntiClockwiseBy90 |> rotateAntiClockwiseBy90
            | Degree270 -> model |> rotateAntiClockwiseBy90 |> rotateAntiClockwiseBy90 |> rotateAntiClockwiseBy90
            | _ -> failwithf "TestDrawBlock.rotateSymbol: Rotation type not matched (should not happen)"
            |> autoRouteAllWires

        /// <summary>Flip a symbol either horizontally or vertically.</summary>
        /// <param name="symLabel">Label of symbol to flip, different from ID.</param>
        /// <param name="flip">Horizontal or vertical, of type SymbolT.FlipType.</param>
        /// <param name="model">Pre-flip sheet model.</param>
        /// <returns>An updated sheet model with the flip.</returns>
        let flipSymbol (symLabel: string) (flip: SymbolT.FlipType) (model: SheetT.Model) : (SheetT.Model) =
            let rotateAntiClockwiseBy90 (model: SheetT.Model): SheetT.Model =
                let symModel = 
                    Optic.get symbolModel_ model
                let symId = 
                    getSymId symLabel symModel
                let updatedSymModel = 
                    SymbolUpdate.updateSymbol (SymbolResizeHelpers.rotateSymbol Degree90) (symId) (Optic.get symbolModel_ model)

                Optic.set symbolModel_ updatedSymModel model

            let flipHorizontal (model: SheetT.Model): SheetT.Model =
                let symModel = 
                    Optic.get symbolModel_ model
                let symId = 
                    getSymId symLabel symModel
                let updatedSymModel = 
                    SymbolUpdate.updateSymbol (SymbolResizeHelpers.flipSymbol SymbolT.FlipHorizontal) (symId) (symModel)

                Optic.set symbolModel_ updatedSymModel model
            
            match flip with
            | SymbolT.FlipHorizontal -> model |> flipHorizontal
            | SymbolT.FlipVertical -> model |> flipHorizontal |> rotateAntiClockwiseBy90 |> rotateAntiClockwiseBy90
            |> autoRouteAllWires

        /// Add a (newly routed) wire, source specifies the Output port, target the Input port.
        /// Return an error if either of the two ports specified is invalid, or if the wire duplicates and existing one.
        /// The wire created will be smart routed but not separated from other wires: for a nice schematic
        /// separateAllWires should be run after  all wires are added.
        /// source, target: respectively the output port and input port to which the wire connects.
        let placeWire (source: SymbolPort) (target: SymbolPort) (model: SheetT.Model) : Result<SheetT.Model,string> =
            let symbols = model.Wire.Symbol.Symbols
            let getPortId (portType:PortType) symPort =
                mapValues symbols
                |> Array.tryFind (fun sym -> caseInvariantEqual sym.Component.Label symPort.Label)
                |> function | Some x -> Ok x | None -> Error "Can't find symbol with label '{symPort.Label}'"
                |> Result.bind (fun sym ->
                    match portType with
                    | PortType.Input -> List.tryItem symPort.PortNumber sym.Component.InputPorts
                    | PortType.Output -> List.tryItem symPort.PortNumber sym.Component.OutputPorts
                    |> function | Some port -> Ok port.Id
                                | None -> Error $"Can't find {portType} port {symPort.PortNumber} on component {symPort.Label}")
            
            match getPortId PortType.Input target, getPortId PortType.Output source with
            | Error e, _ | _, Error e -> Error e
            | Ok inPort, Ok outPort ->
                let newWire = BusWireUpdate.makeNewWire (InputPortId inPort) (OutputPortId outPort) model.Wire
                if model.Wire.Wires |> Map.exists (fun wid wire -> wire.InputPort=newWire.InputPort && wire.OutputPort = newWire.OutputPort) then
                        // wire already exists
                        Error "Can't create wire from {source} to {target} because a wire already exists between those ports"
                else
                     model
                     |> Optic.set (busWireModel_ >-> BusWireT.wireOf_ newWire.WId) newWire
                     |> Ok
            

        /// Run the global wire separation algorithm (should be after all wires have been placed and routed)
        let separateAllWires (model: SheetT.Model) : SheetT.Model =
            model
            |> Optic.map busWireModel_ (BusWireSeparate.updateWireSegmentJumpsAndSeparations (model.Wire.Wires.Keys |> Seq.toList))

        /// Copy testModel into the main Issie Sheet making its contents visible
        let showSheetInIssieSchematic (testModel: SheetT.Model) (dispatch: Dispatch<Msg>) =
            let sheetDispatch sMsg = dispatch (Sheet sMsg)
            dispatch <| UpdateModel (Optic.set sheet_ testModel) // set the Sheet component of the Issie model to make a new schematic.
            sheetDispatch <| SheetT.KeyPress SheetT.CtrlW // Centre & scale the schematic to make all components viewable.


        /// 1. Create a set of circuits from Gen<'a> samples by applying sheetMaker to each sample.
        /// 2. Check each ciruit with sheetChecker.
        /// 3. Return a TestResult record with errors those samples for which sheetChecker returns false,
        /// or where there is an exception.
        /// If there are any test errors display the first in Issie, and its error message on the console.
        /// sheetMaker: generates a SheetT.model from the random sample
        /// sheetChecker n model: n is sample number, model is the genrated model. Return false if test fails.
        let runTestOnSheets
            (name: string)
            (sampleToStartFrom: int)
            (samples : Gen<'a>)
            (sheetMaker: 'a -> SheetT.Model)
            (sheetChecker: int -> SheetT.Model -> string option)
            (dispatch: Dispatch<Msg>)
                : TestResult<'a> =
            let generateAndCheckSheet n = sheetMaker >> sheetChecker n
            let result =
                {
                    Name=name;
                    Samples=samples;
                    StartFrom = sampleToStartFrom
                    Assertion = generateAndCheckSheet
                }
                |> runTests
            match result.TestErrors with
            | [] -> // no errors
                printf $"Test {result.TestName} has PASSED."
            | (n,first):: _ -> // display in Issie editor and print out first error
                printf $"Test {result.TestName} has FAILED on sample {n} with error message:\n{first}"
                match catchException "" sheetMaker (samples.Data n) with
                | Ok sheet -> showSheetInIssieSchematic sheet dispatch
                | Error mess -> ()
            result
//--------------------------------------------------------------------------------------------------//
//----------------------------------------Example Test Circuits using Gen<'a> samples---------------//
//--------------------------------------------------------------------------------------------------//

    open Builder
    /// Sample data based on 11 equidistant points on a horizontal line
    let horizLinePositions =
        fromList [-100..20..100]
        |> map (fun n -> middleOfSheet + {X=float n; Y=0.})

    /// <summary>Sample data based on a square grid of equidistant rows and columns.</summary>
    /// <param name="step">Steps between rows and columns.</param>
    /// <returns>Returns a GeneratedData record of XYPos.</returns>
    let squareGridPositions (step: int) =
        fromList [-100..step..100]
        |> (fun g -> g, g) // duplicate Gen<XYPos> record to pass into GenerateData.product
        ||> GenerateData.product (fun x y -> x, y)
        |> map (fun (dx, dy) -> middleOfSheet + {X=float dx; Y=float dy})

    /// <summary>Generate list of random orientations for the AND gate.</summary>
    /// <param name="sampleSize">Given size of sample data to be generated.</param>
    /// <returns>Returns a GeneratedData record of Flip, Rotation, and XYPos.</returns>
    let randomOrientations (sampleSize: int) =
        let intToRotate (input: int): Option<Rotation> =
            match input with 
            | input when input % 5 = 0 -> Some Degree0
            | input when input % 5 = 1 -> Some Degree90
            | input when input % 5 = 2 -> Some Degree180
            | input when input % 5 = 3 -> Some Degree270
            | input when input % 5 = 4 -> None
            | _ -> failwithf "randomOrientations: intToRotate: input not matched (not supposed to happen)"

        let intToFlip (input: int): Option<SymbolT.FlipType> =
            match input with 
            | input when input % 3 = 0 -> Some SymbolT.FlipHorizontal
            | input when input % 3 = 1 -> Some SymbolT.FlipVertical
            | input when input % 3 = 2 -> None
            | _ -> failwithf "randomOrientations: intToRotate: input not matched (not supposed to happen)"
        
        let andRotations = randomInt 1 1 sampleSize |> map intToRotate
        let andFlips = randomInt 1 1 sampleSize |> map intToFlip

        let andXPos = randomInt -100 1 100 |> truncate sampleSize
        let andYPos = randomInt -100 1 100 |> truncate sampleSize
        let andPos = (andXPos, andYPos) ||> map2 (fun dx dy -> middleOfSheet + {X=float dx; Y=float dy})
        
        map3 (fun rotation flip pos -> rotation, flip, pos) andRotations andFlips andPos

    /// demo test circuit consisting of a DFF & And gate
    let makeTest1Circuit (andPos:XYPos) =
        initSheetModel
        |> placeSymbol "G1" (GateN(And,2)) andPos
        |> Result.bind (placeSymbol "FF1" DFF middleOfSheet)
        |> Result.bind (placeWire (portOf "G1" 0) (portOf "FF1" 0))
        |> Result.bind (placeWire (portOf "FF1" 0) (portOf "G1" 0) )
        |> getOkOrFail

    /// <summary>Create demo circuit consisting of DFF and AND gate, with arbirtary
    /// positioning of AND gate rotation, flip, and position</summary>
    /// <param name="andRotate">AND gate rotate option, will not rotate if passed in None.</param>
    /// <param name="andFlip">AND gate flip option, will not flip if passed in None.</param>
    /// <param name="andPos">Position of the AND gate.</param>
    /// <returns>Updated sheet model.</returns>
    let makeDemoCircuit (andRotate: Option<Rotation>, andFlip: Option<SymbolT.FlipType>, andPos:XYPos) =
        let applyRotate (andRotate: Option<Rotation>) (sheetModel: SheetT.Model): Result<SheetT.Model,string> = 
            match andRotate with
            | None -> Ok sheetModel
            | Some rotate -> Ok (rotateSymbol "G1" rotate sheetModel)
            | _ -> failwithf "makeDemoCircuit: andRotate option not matched (not supposed to happen)"

        let applyFlip (andFlip: Option<SymbolT.FlipType>) (sheetModel: SheetT.Model): Result<SheetT.Model,string> = 
            match andFlip with
            | None -> Ok sheetModel
            | Some flip -> Ok (flipSymbol "G1" flip sheetModel)
            | _ -> failwithf "makeDemoCircuit: andFlip option not matched (not supposed to happen)"

        initSheetModel
        |> placeSymbol "G1" (GateN(And,2)) andPos
        |> Result.bind (placeSymbol "FF1" DFF middleOfSheet)
        |> Result.bind (placeWire (portOf "G1" 0) (portOf "FF1" 0))
        |> Result.bind (placeWire (portOf "FF1" 0) (portOf "G1" 0) )
        |> Result.bind (applyRotate andRotate)
        |> Result.bind (applyFlip andFlip)
        |> getOkOrFail


//------------------------------------------------------------------------------------------------//
//-------------------------Example assertions used to test sheets---------------------------------//
//------------------------------------------------------------------------------------------------//


    module Asserts =

        (* Each assertion function from this module has as inputs the sample number of the current test and the corresponding schematic sheet.
           It returns a boolean indicating (true) that the test passes or 9false) that the test fails. The sample numbr is included to make it
           easy to document tests and so that any specific sampel schematic can easily be displayed using failOnSampleNumber. *)

        /// Ignore sheet and fail on the specified sample, useful for displaying a given sample
        let failOnSampleNumber (sampleToFail :int) (sample: int) _sheet =
            if sampleToFail = sample then
                Some $"Failing forced on Sample {sampleToFail}."
            else
                None

        /// Fails all tests: useful to show in sequence all the sheets generated in a test
        let failOnAllTests (sample: int) _ =
            Some <| $"Sample {sample}"

        /// Fail when sheet contains a wire segment that overlaps (or goes too close to) a symbol outline  
        let failOnWireIntersectsSymbol (sample: int) (sheet: SheetT.Model) =
            let wireModel = sheet.Wire
            wireModel.Wires
            |> Map.exists (fun _ wire -> BusWireRoute.findWireSymbolIntersections wireModel wire <> [])
            |> (function | true -> Some $"Wire intersects a symbol outline in Sample {sample}"
                         | false -> None)

        /// Fail when sheet contains two symbols which overlap
        let failOnSymbolIntersectsSymbol (sample: int) (sheet: SheetT.Model) =
            let wireModel = sheet.Wire
            let boxes =
                mapValues sheet.BoundingBoxes
                |> Array.toList
                |> List.mapi (fun n box -> n,box)
            List.allPairs boxes boxes 
            |> List.exists (fun ((n1,box1),(n2,box2)) -> (n1 <> n2) && BlockHelpers.overlap2DBox box1 box2)
            |> (function | true -> Some $"Symbol outline intersects another symbol outline in Sample {sample}"
                         | false -> None)



//---------------------------------------------------------------------------------------//
//-----------------------------Demo tests on Draw Block code-----------------------------//
//---------------------------------------------------------------------------------------//

    module Tests =

        /// Allow test errors to be viewed in sequence by recording the current error
        /// in the Issie Model (field DrawblockTestState). This contains all Issie persistent state.
        let recordPositionInTest (testNumber: int) (dispatch: Dispatch<Msg>) (result: TestResult<'a>) =
            dispatch <| UpdateDrawBlockTestState(fun _ ->
                match result.TestErrors with
                | [] ->
                    printf "Test finished"
                    None
                | (numb, _) :: _ ->
                    printf $"Sample {numb}"
                    Some { LastTestNumber=testNumber; LastTestSampleIndex= numb})
            
        /// Example test: Horizontally positioned AND + DFF: fail on sample 0
        let test1 testNum firstSample dispatch =
            runTestOnSheets
                "Horizontally positioned AND + DFF: fail on sample 0"
                firstSample
                horizLinePositions
                makeTest1Circuit
                (Asserts.failOnSampleNumber 0)
                dispatch
            |> recordPositionInTest testNum dispatch

        /// Example test: Horizontally positioned AND + DFF: fail on sample 10
        let test2 testNum firstSample dispatch =
            runTestOnSheets
                "Horizontally positioned AND + DFF: fail on sample 10"
                firstSample
                horizLinePositions
                makeTest1Circuit
                (Asserts.failOnSampleNumber 10)
                dispatch
            |> recordPositionInTest testNum dispatch

        /// Example test: Horizontally positioned AND + DFF: fail on symbols intersect
        let test3 testNum firstSample dispatch =
            runTestOnSheets
                "Horizontally positioned AND + DFF: fail on symbols intersect"
                firstSample
                horizLinePositions
                makeTest1Circuit
                Asserts.failOnSymbolIntersectsSymbol
                dispatch
            |> recordPositionInTest testNum dispatch

        /// Example test: Horizontally positioned AND + DFF: fail all tests
        let test4 testNum firstSample dispatch =
            runTestOnSheets
                "Horizontally positioned AND + DFF: fail all tests"
                firstSample
                horizLinePositions
                makeTest1Circuit
                Asserts.failOnAllTests
                dispatch
            |> recordPositionInTest testNum dispatch
        
        /// <summary>[Tick3 Step7] Test 5: Square-grid positioned AND + DFF (20px step), 
        /// fail all tests. Useful for displaying all grid.</summary>
        let test5 testNum firstSample dispatch =
            runTestOnSheets
                "Test 5: Square-grid positioned AND + DFF (20px step), fail all tests."
                firstSample
                (squareGridPositions 20)
                makeTest1Circuit
                Asserts.failOnAllTests
                dispatch
            |> recordPositionInTest testNum dispatch

        /// <summary>[Tick3 Step7] Test 6: Square-grid positioned AND + DFF without symbol 
        /// collision (10px step), fail on wire intersect. Useful for displaying faults
        /// in auto-routing.</summary>
        /// <remarks>Could possibly be done in more abstracted manner. Code taken from previous
        /// Assertion module to save time.</remarks>
        let test6 testNum firstSample dispatch =
            let filterSymbolCollisionFromPosition (pos: XYPos): bool =
                let sheet = makeTest1Circuit pos
                let boxes = mapValues sheet.BoundingBoxes |> Array.toList |> List.mapi (fun n box -> n,box)
                List.allPairs boxes boxes 
                |> List.exists (fun ((n1,box1),(n2,box2)) -> (n1 <> n2) && BlockHelpers.overlap2DBox box1 box2)
                |> (fun x -> not x) // keep if no intersection

            runTestOnSheets
                "Test 6: Square-grid positioned AND + DFF without symbol collision (10px step), fail on wire intersect."
                firstSample
                (GenerateData.filter filterSymbolCollisionFromPosition (squareGridPositions 10))
                makeTest1Circuit
                Asserts.failOnWireIntersectsSymbol
                dispatch
            |> recordPositionInTest testNum dispatch

        /// <summary>[Tick3 Step10] Test 7: Randomly positioned and oriented AND + DFF,
        /// fails on all tests. Total of 100 samples. Useful for demonstrating working 
        /// randomness and arbitrary orientation.</summary>
        let test7 testNum firstSample dispatch =
            runTestOnSheets
                "Test 7: Randomly positioned and oriented AND + DFF, fails on all tests."
                firstSample
                (randomOrientations 100)
                makeDemoCircuit
                Asserts.failOnAllTests
                dispatch
            |> recordPositionInTest testNum dispatch

        /// <summary>[Tick3 Step10] Test 8: Randomly positioned and oriented AND + DFF
        /// without symbol collisions, fails on wire collision. Total of 100 samples. 
        /// Useful for displaying faults in auto-routing.</summary>
        /// <remarks>Could possibly be done in more abstracted manner. Code taken from previous
        /// Assertion module to save time.</remarks>
        let test8 testNum firstSample dispatch =
            let filterSymbolCollisionFromOrientation (_, _, pos: XYPos): bool =
                let sheet = makeTest1Circuit pos
                let boxes = mapValues sheet.BoundingBoxes |> Array.toList |> List.mapi (fun n box -> n,box)
                List.allPairs boxes boxes 
                |> List.exists (fun ((n1,box1),(n2,box2)) -> (n1 <> n2) && BlockHelpers.overlap2DBox box1 box2)
                |> (fun x -> not x) // keep if no intersection
            
            runTestOnSheets
                "Test 8: Randomly positioned and oriented AND + DFF without symbol collisions, fails on wire collision."
                firstSample
                (GenerateData.filter filterSymbolCollisionFromOrientation (randomOrientations 100))
                makeDemoCircuit
                Asserts.failOnWireIntersectsSymbol
                dispatch
            |> recordPositionInTest testNum dispatch

        /// List of tests available which can be run ftom Issie File Menu.
        /// The first 9 tests can also be run via Ctrl-n accelerator keys as shown on menu
        let testsToRunFromSheetMenu : (string * (int -> int -> Dispatch<Msg> -> Unit)) list =
            // Change names and test functions as required
            // delete unused tests from list
            [
                "Test1", test1 // example: horiz
                "Test2", test2 // example: horiz
                "Test3", test3 // example: horiz
                "Test4", test4 // example: horiz
                "Test5", test5 // tick 3 step 7: fail on all tests and display grid
                "Test6", test6 // tick 3 step 7: filter symbol collision and fail on wire interesect
                "Test7", test7 // tick 3 step 10: fail on all tests and display random orientation
                "Test8", test8 // tick 3 step 10: filter symbol collision and fail on wire interesect
                "Next Test Error", fun _ _ _ -> printf "Next Error:" // Go to the nexterror in a test
            ]

        /// Display the next error in a previously started test
        let nextError (testName, testFunc) firstSampleToTest dispatch =
            let testNum =
                testsToRunFromSheetMenu
                |> List.tryFindIndex (fun (name,_) -> name = testName)
                |> Option.defaultValue 0
            testFunc testNum firstSampleToTest dispatch

        /// common function to execute any test.
        /// testIndex: index of test in testsToRunFromSheetMenu
        let testMenuFunc (testIndex: int) (dispatch: Dispatch<Msg>) (model: Model) =
            let name,func = testsToRunFromSheetMenu[testIndex] 
            printf "%s" name
            match name, model.DrawBlockTestState with
            | "Next Test Error", Some state ->
                nextError testsToRunFromSheetMenu[state.LastTestNumber] (state.LastTestSampleIndex+1) dispatch
            | "Next Test Error", None ->
                printf "Test Finished"
                ()
            | _ ->
                func testIndex 0 dispatch
