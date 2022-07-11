// Generated automatically by nearley, version 2.20.1
// http://github.com/Hardmath123/nearley
(function () {
    function id(x) { return x[0]; }
    var grammar = {
        Lexer: undefined,
        ParserRules: [
        {"name": "PROGRAM", "symbols": ["MODULE"], "postprocess": function(d) {return {Type: "program", Module: d[0]};}},
        {"name": "MODULE$string$1", "symbols": [{"literal":"m"}, {"literal":"o"}, {"literal":"d"}, {"literal":"u"}, {"literal":"l"}, {"literal":"e"}], "postprocess": function joiner(d) {return d.join('');}},
        {"name": "MODULE$ebnf$1", "symbols": ["LIST_OF_PORTS"], "postprocess": id},
        {"name": "MODULE$ebnf$1", "symbols": [], "postprocess": function(d) {return null;}},
        {"name": "MODULE", "symbols": ["MODULE$string$1", "__", "NAME_OF_MODULE", "_", {"literal":"("}, "_", "MODULE$ebnf$1", "_", {"literal":")"}, "_", {"literal":";"}, "_", "MODULE_ITEMS", "_", "endmodule", "_"], "postprocess": function(d) {return {Type: "module", ModuleName: d[2], PortList: d[6], ModuleItems: d[12]};}},
        {"name": "NAME_OF_MODULE", "symbols": ["IDENTIFIER"], "postprocess": id},
        {"name": "LIST_OF_PORTS", "symbols": ["PORT", "_", {"literal":","}, "_", "LIST_OF_PORTS"], "postprocess": function(d, l, reject) {return {Type: "port_list", Head: d[0], Tail: d[4], Location: l};}},
        {"name": "LIST_OF_PORTS", "symbols": ["PORT"], "postprocess": function(d,l,reject) {return {Type: "port_list", Head: d[0], Tail: null, Location: l};}},
        {"name": "PORT", "symbols": ["IDENTIFIER"], "postprocess": function(d) {return {Type: "port", Port: d[0]};}},
        {"name": "MODULE_ITEMS$ebnf$1", "symbols": []},
        {"name": "MODULE_ITEMS$ebnf$1", "symbols": ["MODULE_ITEMS$ebnf$1", "MODULE_ITEM"], "postprocess": function arrpush(d) {return d[0].concat([d[1]]);}},
        {"name": "MODULE_ITEMS", "symbols": ["MODULE_ITEMS$ebnf$1"], "postprocess": function(d) {return {Type: "module_items", ItemList: d[0]};}},
        {"name": "MODULE_ITEM", "symbols": ["INPUT_DECL", "_"], "postprocess": function(d,l, reject) {return {Type: "item", ItemType: "input_decl", IODecl: d[0], ParamDecl: null, Statement: null, Location: l};}},
        {"name": "MODULE_ITEM", "symbols": ["OUTPUT_DECL", "_"], "postprocess": function(d,l, reject) {return {Type: "item", ItemType: "output_decl", IODecl: d[0], ParamDecl: null, Statement: null, Location: l};}},
        {"name": "MODULE_ITEM", "symbols": ["PARAMETER_DECL", "_"], "postprocess": function(d,l, reject) {return {Type: "item", ItemType: "parameter_decl", IODecl: null, ParamDecl: d[0], Statement: null, Location: l};}},
        {"name": "MODULE_ITEM", "symbols": ["STATEMENT", "_"], "postprocess": function(d,l, reject) {return {Type: "item", ItemType: "statement", IODecl: null, ParamDecl: null, Statement: d[0], Location: l};}},
        {"name": "INPUT_DECL$ebnf$1", "symbols": ["RANGE"], "postprocess": id},
        {"name": "INPUT_DECL$ebnf$1", "symbols": [], "postprocess": function(d) {return null;}},
        {"name": "INPUT_DECL", "symbols": ["input", "__", "INPUT_DECL$ebnf$1", "_", "LIST_OF_VARIABLES", "_", {"literal":";"}], "postprocess": function(d) {return {Type: "declaration", DeclarationType: "input", Range: d[2], Variables: d[4]};}},
        {"name": "OUTPUT_DECL$ebnf$1", "symbols": ["RANGE"], "postprocess": id},
        {"name": "OUTPUT_DECL$ebnf$1", "symbols": [], "postprocess": function(d) {return null;}},
        {"name": "OUTPUT_DECL", "symbols": ["output", "__", "OUTPUT_DECL$ebnf$1", "_", "LIST_OF_VARIABLES", "_", {"literal":";"}], "postprocess": function(d) {return {Type: "declaration", DeclarationType: "output", Range: d[2], Variables: d[4]};}},
        {"name": "LIST_OF_VARIABLES", "symbols": ["NAME_OF_VARIABLE", "_", {"literal":","}, "_", "LIST_OF_VARIABLES"], "postprocess": function(d) {return {Type: "variable_list", Head: d[0], Tail: d[4]};}},
        {"name": "LIST_OF_VARIABLES", "symbols": ["NAME_OF_VARIABLE"], "postprocess": function(d) {return {Type: "variable_list", Head: d[0], Tail: null};}},
        {"name": "NAME_OF_VARIABLE", "symbols": ["IDENTIFIER"], "postprocess": function(d) {return {Type: "variable", Name: d[0]};}},
        {"name": "RANGE", "symbols": [{"literal":"["}, "UNSIGNED_NUMBER", {"literal":":"}, "UNSIGNED_NUMBER", {"literal":"]"}], "postprocess": function(d,l,reject) {return {Type: "range", Start: d[1], End: d[3], Location: l};}},
        {"name": "PARAMETER_DECL", "symbols": ["parameter", "__", "PARAM_ASSIGNMENT", "_", {"literal":";"}], "postprocess": function(d) {return {Type: "declaration", DeclarationType: "parameter", Parameter: d[2]};}},
        {"name": "PARAM_ASSIGNMENT", "symbols": ["IDENTIFIER", "_", {"literal":"="}, "_", "NUMBER"], "postprocess": function(d) {return {Type: "parameter_assignment", Identifier: d[0], RHS: d[4]};}},
        {"name": "STATEMENT", "symbols": ["assign", "__", "ASSIGNMENT", "_", {"literal":";"}], "postprocess": function(d) {return {Type: "statement", StatementType: "assign", Assignment: d[2]};}},
        {"name": "STATEMENT", "symbols": ["wire", "__", "WIRE_ASSIGNMENT", "_", {"literal":";"}], "postprocess": function(d) {return {Type: "statement", StatementType: "wire", Assignment: d[2]};}},
        {"name": "ASSIGNMENT", "symbols": ["L_VALUE", "_", {"literal":"="}, "_", "EXPRESSION"], "postprocess": function(d) {return {Type: "assign", LHS: d[0], RHS: d[4]};}},
        {"name": "WIRE_ASSIGNMENT", "symbols": ["WIRE_L_VALUE", "_", {"literal":"="}, "_", "EXPRESSION"], "postprocess": function(d) {return {Type: "wire", LHS: d[0], RHS: d[4]};}},
        {"name": "WIRE_L_VALUE", "symbols": ["IDENTIFIER"], "postprocess": function(d) {return {Type: "l_value", PrimaryType: "identifier", BitsStart: null, BitsEnd: null, Primary: d[0]};}},
        {"name": "WIRE_L_VALUE", "symbols": [{"literal":"["}, "UNSIGNED_NUMBER", {"literal":":"}, "UNSIGNED_NUMBER", {"literal":"]"}, "_", "IDENTIFIER"], "postprocess": function(d) {return {Type: "l_value", PrimaryType: "identifier_bits", BitsStart: d[1], BitsEnd: d[3], Primary: d[6]};}},
        {"name": "L_VALUE", "symbols": ["IDENTIFIER"], "postprocess": function(d) {return {Type: "l_value", PrimaryType: "identifier", BitsStart: null, BitsEnd: null, Primary: d[0]};}},
        {"name": "L_VALUE", "symbols": ["IDENTIFIER", "_", {"literal":"["}, "UNSIGNED_NUMBER", {"literal":"]"}], "postprocess": function(d) {return {Type: "l_value", PrimaryType: "identifier_bit", BitsStart: d[3], BitsEnd: d[3], Primary: d[0]};}},
        {"name": "L_VALUE", "symbols": ["IDENTIFIER", "_", {"literal":"["}, "UNSIGNED_NUMBER", {"literal":":"}, "UNSIGNED_NUMBER", {"literal":"]"}], "postprocess": function(d) {return {Type: "l_value", PrimaryType: "identifier_bits", BitsStart: d[3], BitsEnd: d[5], Primary: d[0]};}},
        {"name": "EXPRESSION", "symbols": ["CONDITIONAL"], "postprocess": id},
        {"name": "CONDITIONAL", "symbols": ["LOGICAL_OR", "_", {"literal":"?"}, "_", "CONDITIONAL"], "postprocess": function(d) {return {Type: "conditional_cond", Operator:d[2], Head: d[0], Tail: d[4]};}},
        {"name": "CONDITIONAL", "symbols": ["LOGICAL_OR", "_", {"literal":":"}, "_", "LOGICAL_OR"], "postprocess": function(d) {return {Type: "conditional_result", Operator:d[2], Head: d[0], Tail: d[4]};}},
        {"name": "CONDITIONAL", "symbols": ["LOGICAL_OR"], "postprocess": id},
        {"name": "LOGICAL_OR$string$1", "symbols": [{"literal":"|"}, {"literal":"|"}], "postprocess": function joiner(d) {return d.join('');}},
        {"name": "LOGICAL_OR", "symbols": ["LOGICAL_AND", "_", "LOGICAL_OR$string$1", "_", "LOGICAL_OR"], "postprocess": function(d) {return {Type: "logical_OR", Operator:d[2], Head: d[0], Tail: d[4]};}},
        {"name": "LOGICAL_OR", "symbols": ["LOGICAL_AND"], "postprocess": id},
        {"name": "LOGICAL_AND$string$1", "symbols": [{"literal":"&"}, {"literal":"&"}], "postprocess": function joiner(d) {return d.join('');}},
        {"name": "LOGICAL_AND", "symbols": ["BITWISE_OR", "_", "LOGICAL_AND$string$1", "_", "LOGICAL_AND"], "postprocess": function(d) {return {Type: "logical_AND", Operator:d[2], Head: d[0], Tail: d[4]};}},
        {"name": "LOGICAL_AND", "symbols": ["BITWISE_OR"], "postprocess": id},
        {"name": "BITWISE_OR", "symbols": ["BITWISE_XOR", "_", {"literal":"|"}, "_", "BITWISE_OR"], "postprocess": function(d) {return {Type: "bitwise_OR", Operator:d[2], Head: d[0], Tail: d[4]};}},
        {"name": "BITWISE_OR", "symbols": ["BITWISE_XOR"], "postprocess": id},
        {"name": "BITWISE_XOR", "symbols": ["BITWISE_AND", "_", "XOR_XNOR_OPERATOR", "_", "BITWISE_XOR"], "postprocess": function(d) {return {Type: "bitwise_XOR", Operator:d[2][0], Head: d[0], Tail: d[4]};}},
        {"name": "BITWISE_XOR", "symbols": ["BITWISE_AND"], "postprocess": id},
        {"name": "BITWISE_AND", "symbols": ["LOGICAL_SHIFT", "_", {"literal":"&"}, "_", "BITWISE_AND"], "postprocess": function(d) {return {Type: "bitwise_AND", Operator:d[2], Head: d[0], Tail: d[4]};}},
        {"name": "BITWISE_AND", "symbols": ["LOGICAL_SHIFT"], "postprocess": id},
        {"name": "LOGICAL_SHIFT", "symbols": ["ADDITIVE", "_", "SHIFT_OPERATOR", "_", "UNSIGNED_REDUCTED"], "postprocess": function(d) {return {Type: "SHIFT", Operator:d[2][0], Head: d[0], Tail: d[4]};}},
        {"name": "LOGICAL_SHIFT", "symbols": ["ADDITIVE"], "postprocess": id},
        {"name": "ADDITIVE", "symbols": ["REDUCTION_OR_NEGATION", "_", "ADDITIVE_OPERATOR", "_", "ADDITIVE"], "postprocess": function(d) {return {Type: "additive", Operator:d[2][0], Head: d[0], Tail: d[4]};}},
        {"name": "ADDITIVE", "symbols": ["REDUCTION_OR_NEGATION"], "postprocess": id},
        {"name": "REDUCTION_OR_NEGATION", "symbols": ["UNARY_OPERATOR", "_", "UNARY"], "postprocess": function(d) {return {Type: "reduction", Operator:d[0][0], Unary: d[2]};}},
        {"name": "REDUCTION_OR_NEGATION", "symbols": [{"literal":"~"}, "_", "UNARY"], "postprocess": function(d) {return {Type: "negation", Operator: "~", Unary: d[2]};}},
        {"name": "REDUCTION_OR_NEGATION", "symbols": ["UNARY"], "postprocess": function(d) {return {Type: "unary", Unary: d[0]};}},
        {"name": "UNARY", "symbols": ["PRIMARY"], "postprocess": function(d) {return {Type: "primary", Primary: d[0], Number: null, Expression: null};}},
        {"name": "UNARY", "symbols": ["NUMBER"], "postprocess": function(d) {return {Type: "number", Primary: null, Number: d[0], Expression: null};}},
        {"name": "UNARY", "symbols": [{"literal":"("}, "_", "BITWISE_OR", "_", {"literal":")"}], "postprocess": function(d) {return {Type: "parenthesis", Primary: null, Number: null, Expression: d[2]};}},
        {"name": "UNARY", "symbols": [{"literal":"{"}, "_", "LIST_OF_UNARIES", "_", {"literal":"}"}], "postprocess": function(d) {return {Type: "concat", Primary: null, Number: null, Expression: d[2]};}},
        {"name": "LIST_OF_UNARIES", "symbols": ["EXPRESSION", "_", {"literal":","}, "_", "LIST_OF_UNARIES"], "postprocess": function(d) {return {Type: "unary_list", Head : d[0], Tail: d[4]};}},
        {"name": "LIST_OF_UNARIES", "symbols": ["EXPRESSION"], "postprocess": function(d) {return {Type: "unary_list", Head: d[0], Tail: null};}},
        {"name": "UNSIGNED_REDUCTED", "symbols": ["UNSIGNED_UNARY"], "postprocess": function(d) {return {Type: "unary", Unary: d[0]};}},
        {"name": "UNSIGNED_UNARY", "symbols": ["U_NUMBER"], "postprocess": function(d) {return {Type: "number", Primary: null, Number: d[0], Expression: null};}},
        {"name": "U_NUMBER", "symbols": ["UNSIGNED_NUMBER"], "postprocess": function(d,l,reject) {return {Type: "number", NumberType: "decimal", Bits: null, Base: null, UnsignedNumber: d[0], AllNumber: null, Location: l};}},
        {"name": "ADDITIVE_OPERATOR", "symbols": [{"literal":"+"}]},
        {"name": "ADDITIVE_OPERATOR", "symbols": [{"literal":"-"}], "postprocess": function(d) {return d;}},
        {"name": "XOR_XNOR_OPERATOR", "symbols": [{"literal":"^"}]},
        {"name": "XOR_XNOR_OPERATOR$string$1", "symbols": [{"literal":"~"}, {"literal":"^"}], "postprocess": function joiner(d) {return d.join('');}},
        {"name": "XOR_XNOR_OPERATOR", "symbols": ["XOR_XNOR_OPERATOR$string$1"]},
        {"name": "XOR_XNOR_OPERATOR$string$2", "symbols": [{"literal":"^"}, {"literal":"~"}], "postprocess": function joiner(d) {return d.join('');}},
        {"name": "XOR_XNOR_OPERATOR", "symbols": ["XOR_XNOR_OPERATOR$string$2"], "postprocess": function(d) {return d;}},
        {"name": "SHIFT_OPERATOR$string$1", "symbols": [{"literal":"<"}, {"literal":"<"}], "postprocess": function joiner(d) {return d.join('');}},
        {"name": "SHIFT_OPERATOR", "symbols": ["SHIFT_OPERATOR$string$1"]},
        {"name": "SHIFT_OPERATOR$string$2", "symbols": [{"literal":">"}, {"literal":">"}], "postprocess": function joiner(d) {return d.join('');}},
        {"name": "SHIFT_OPERATOR", "symbols": ["SHIFT_OPERATOR$string$2"]},
        {"name": "SHIFT_OPERATOR$string$3", "symbols": [{"literal":">"}, {"literal":">"}, {"literal":">"}], "postprocess": function joiner(d) {return d.join('');}},
        {"name": "SHIFT_OPERATOR", "symbols": ["SHIFT_OPERATOR$string$3"], "postprocess": function(d) {return d;}},
        {"name": "UNARY_OPERATOR", "symbols": [{"literal":"!"}]},
        {"name": "UNARY_OPERATOR", "symbols": [{"literal":"&"}]},
        {"name": "UNARY_OPERATOR$string$1", "symbols": [{"literal":"~"}, {"literal":"&"}], "postprocess": function joiner(d) {return d.join('');}},
        {"name": "UNARY_OPERATOR", "symbols": ["UNARY_OPERATOR$string$1"]},
        {"name": "UNARY_OPERATOR", "symbols": [{"literal":"|"}]},
        {"name": "UNARY_OPERATOR$string$2", "symbols": [{"literal":"~"}, {"literal":"|"}], "postprocess": function joiner(d) {return d.join('');}},
        {"name": "UNARY_OPERATOR", "symbols": ["UNARY_OPERATOR$string$2"]},
        {"name": "UNARY_OPERATOR", "symbols": [{"literal":"^"}]},
        {"name": "UNARY_OPERATOR$string$3", "symbols": [{"literal":"~"}, {"literal":"^"}], "postprocess": function joiner(d) {return d.join('');}},
        {"name": "UNARY_OPERATOR", "symbols": ["UNARY_OPERATOR$string$3"], "postprocess": function(d) {return d[0].join('');}},
        {"name": "BINARY_OPERATOR", "symbols": [{"literal":"+"}]},
        {"name": "BINARY_OPERATOR", "symbols": [{"literal":"-"}]},
        {"name": "BINARY_OPERATOR", "symbols": [{"literal":"*"}]},
        {"name": "BINARY_OPERATOR", "symbols": [{"literal":"/"}]},
        {"name": "BINARY_OPERATOR", "symbols": [{"literal":"%"}]},
        {"name": "BINARY_OPERATOR$string$1", "symbols": [{"literal":"="}, {"literal":"="}], "postprocess": function joiner(d) {return d.join('');}},
        {"name": "BINARY_OPERATOR", "symbols": ["BINARY_OPERATOR$string$1"]},
        {"name": "BINARY_OPERATOR$string$2", "symbols": [{"literal":"!"}, {"literal":"="}], "postprocess": function joiner(d) {return d.join('');}},
        {"name": "BINARY_OPERATOR", "symbols": ["BINARY_OPERATOR$string$2"]},
        {"name": "BINARY_OPERATOR$string$3", "symbols": [{"literal":"="}, {"literal":"="}, {"literal":"="}], "postprocess": function joiner(d) {return d.join('');}},
        {"name": "BINARY_OPERATOR", "symbols": ["BINARY_OPERATOR$string$3"]},
        {"name": "BINARY_OPERATOR$string$4", "symbols": [{"literal":"!"}, {"literal":"="}, {"literal":"="}], "postprocess": function joiner(d) {return d.join('');}},
        {"name": "BINARY_OPERATOR", "symbols": ["BINARY_OPERATOR$string$4"]},
        {"name": "BINARY_OPERATOR$string$5", "symbols": [{"literal":"&"}, {"literal":"&"}], "postprocess": function joiner(d) {return d.join('');}},
        {"name": "BINARY_OPERATOR", "symbols": ["BINARY_OPERATOR$string$5"]},
        {"name": "BINARY_OPERATOR$string$6", "symbols": [{"literal":"|"}, {"literal":"|"}], "postprocess": function joiner(d) {return d.join('');}},
        {"name": "BINARY_OPERATOR", "symbols": ["BINARY_OPERATOR$string$6"]},
        {"name": "BINARY_OPERATOR", "symbols": [{"literal":"<"}]},
        {"name": "BINARY_OPERATOR$string$7", "symbols": [{"literal":"<"}, {"literal":"="}], "postprocess": function joiner(d) {return d.join('');}},
        {"name": "BINARY_OPERATOR", "symbols": ["BINARY_OPERATOR$string$7"]},
        {"name": "BINARY_OPERATOR", "symbols": [{"literal":">"}]},
        {"name": "BINARY_OPERATOR$string$8", "symbols": [{"literal":">"}, {"literal":"="}], "postprocess": function joiner(d) {return d.join('');}},
        {"name": "BINARY_OPERATOR", "symbols": ["BINARY_OPERATOR$string$8"]},
        {"name": "BINARY_OPERATOR", "symbols": [{"literal":"&"}]},
        {"name": "BINARY_OPERATOR", "symbols": [{"literal":"|"}]},
        {"name": "BINARY_OPERATOR", "symbols": [{"literal":"^"}]},
        {"name": "BINARY_OPERATOR$string$9", "symbols": [{"literal":"^"}, {"literal":"~"}], "postprocess": function joiner(d) {return d.join('');}},
        {"name": "BINARY_OPERATOR", "symbols": ["BINARY_OPERATOR$string$9"]},
        {"name": "QUESTION_MARK", "symbols": [{"literal":"?"}]},
        {"name": "PRIMARY", "symbols": ["IDENTIFIER"], "postprocess": function(d) {return {Type: "primary", PrimaryType: "identifier", BitsStart: null, BitsEnd: null, Primary: d[0]};}},
        {"name": "PRIMARY", "symbols": ["IDENTIFIER", "_", {"literal":"["}, "UNSIGNED_NUMBER", {"literal":"]"}], "postprocess": function(d) {return {Type: "primary", PrimaryType: "identifier_bit", BitsStart: d[3], BitsEnd: d[3], Primary: d[0]};}},
        {"name": "PRIMARY", "symbols": ["IDENTIFIER", "_", {"literal":"["}, "UNSIGNED_NUMBER", {"literal":":"}, "UNSIGNED_NUMBER", {"literal":"]"}], "postprocess": function(d) {return {Type: "primary", PrimaryType: "identifier_bits", BitsStart: d[3], BitsEnd: d[5], Primary: d[0]};}},
        {"name": "NUMBER$string$1", "symbols": [{"literal":"'"}, {"literal":"h"}], "postprocess": function joiner(d) {return d.join('');}},
        {"name": "NUMBER", "symbols": ["UNSIGNED_NUMBER", "NUMBER$string$1", "ALL_NUMERIC"], "postprocess": function(d,l,reject) {return {Type: "number", NumberType: "all", Bits: d[0], Base: "'h", UnsignedNumber: null, AllNumber: d[2], Location: l};}},
        {"name": "NUMBER$string$2", "symbols": [{"literal":"'"}, {"literal":"b"}], "postprocess": function joiner(d) {return d.join('');}},
        {"name": "NUMBER", "symbols": ["UNSIGNED_NUMBER", "NUMBER$string$2", "BINARY_NUMBER"], "postprocess": function(d,l,reject) {return {Type: "number", NumberType: "all", Bits: d[0], Base: "'b", UnsignedNumber: null, AllNumber: d[2], Location: l};}},
        {"name": "NUMBER$string$3", "symbols": [{"literal":"'"}, {"literal":"d"}], "postprocess": function joiner(d) {return d.join('');}},
        {"name": "NUMBER", "symbols": ["UNSIGNED_NUMBER", "NUMBER$string$3", "UNSIGNED_NUMBER"], "postprocess": function(d,l,reject) {return {Type: "number", NumberType: "all", Bits: d[0], Base: "'d", UnsignedNumber: null, AllNumber: d[2], Location: l};}},
        {"name": "UNSIGNED_NUMBER$ebnf$1", "symbols": [/[0-9]/]},
        {"name": "UNSIGNED_NUMBER$ebnf$1", "symbols": ["UNSIGNED_NUMBER$ebnf$1", /[0-9]/], "postprocess": function arrpush(d) {return d[0].concat([d[1]]);}},
        {"name": "UNSIGNED_NUMBER", "symbols": ["UNSIGNED_NUMBER$ebnf$1"], "postprocess": function(d) {return d[0].join('');}},
        {"name": "ALL_NUMERIC$ebnf$1", "symbols": [/[0-9a-fA-F]/]},
        {"name": "ALL_NUMERIC$ebnf$1", "symbols": ["ALL_NUMERIC$ebnf$1", /[0-9a-fA-F]/], "postprocess": function arrpush(d) {return d[0].concat([d[1]]);}},
        {"name": "ALL_NUMERIC", "symbols": ["ALL_NUMERIC$ebnf$1"], "postprocess": function(d) {return d[0].join('');}},
        {"name": "BINARY_NUMBER$ebnf$1", "symbols": [/[0-1]/]},
        {"name": "BINARY_NUMBER$ebnf$1", "symbols": ["BINARY_NUMBER$ebnf$1", /[0-1]/], "postprocess": function arrpush(d) {return d[0].concat([d[1]]);}},
        {"name": "BINARY_NUMBER", "symbols": ["BINARY_NUMBER$ebnf$1"], "postprocess": function(d) {return d[0].join('');}},
        {"name": "BASE$string$1", "symbols": [{"literal":"'"}, {"literal":"b"}], "postprocess": function joiner(d) {return d.join('');}},
        {"name": "BASE", "symbols": ["BASE$string$1"]},
        {"name": "BASE$string$2", "symbols": [{"literal":"'"}, {"literal":"h"}], "postprocess": function joiner(d) {return d.join('');}},
        {"name": "BASE", "symbols": ["BASE$string$2"], "postprocess": id},
        {"name": "CONCAT", "symbols": ["EXPRESSION", "_", {"literal":","}, "_", "CONCAT"], "postprocess": function(d) {return {Type: "concatenation_list", Head: d[0], Tail: d[4]};}},
        {"name": "CONCAT", "symbols": ["EXPRESSION"], "postprocess": function(d) {return {Type: "concatenation_list", Head: d[0], Tail: null};}},
        {"name": "input$string$1", "symbols": [{"literal":"i"}, {"literal":"n"}, {"literal":"p"}, {"literal":"u"}, {"literal":"t"}], "postprocess": function joiner(d) {return d.join('');}},
        {"name": "input", "symbols": ["input$string$1"], "postprocess": id},
        {"name": "output$string$1", "symbols": [{"literal":"o"}, {"literal":"u"}, {"literal":"t"}, {"literal":"p"}, {"literal":"u"}, {"literal":"t"}], "postprocess": function joiner(d) {return d.join('');}},
        {"name": "output", "symbols": ["output$string$1"], "postprocess": id},
        {"name": "parameter$string$1", "symbols": [{"literal":"p"}, {"literal":"a"}, {"literal":"r"}, {"literal":"a"}, {"literal":"m"}, {"literal":"e"}, {"literal":"t"}, {"literal":"e"}, {"literal":"r"}], "postprocess": function joiner(d) {return d.join('');}},
        {"name": "parameter", "symbols": ["parameter$string$1"], "postprocess": id},
        {"name": "assign$string$1", "symbols": [{"literal":"a"}, {"literal":"s"}, {"literal":"s"}, {"literal":"i"}, {"literal":"g"}, {"literal":"n"}], "postprocess": function joiner(d) {return d.join('');}},
        {"name": "assign", "symbols": ["assign$string$1"], "postprocess": id},
        {"name": "wire$string$1", "symbols": [{"literal":"w"}, {"literal":"i"}, {"literal":"r"}, {"literal":"e"}], "postprocess": function joiner(d) {return d.join('');}},
        {"name": "wire", "symbols": ["wire$string$1"], "postprocess": id},
        {"name": "endmodule$string$1", "symbols": [{"literal":"e"}, {"literal":"n"}, {"literal":"d"}, {"literal":"m"}, {"literal":"o"}, {"literal":"d"}, {"literal":"u"}, {"literal":"l"}, {"literal":"e"}], "postprocess": function joiner(d) {return d.join('');}},
        {"name": "endmodule", "symbols": ["endmodule$string$1"], "postprocess": id},
        {"name": "IDENTIFIER$ebnf$1", "symbols": []},
        {"name": "IDENTIFIER$ebnf$1", "symbols": ["IDENTIFIER$ebnf$1", /[a-zA-Z_0-9]/], "postprocess": function arrpush(d) {return d[0].concat([d[1]]);}},
        {"name": "IDENTIFIER", "symbols": [/[a-zA-Z]/, "IDENTIFIER$ebnf$1"], "postprocess": 
            function(d,l, reject) {
                const keywords = ["alias","and","assert","assign","assume","automatic","before","begin","bind","bins","binsof","bit","break","buf","bufif0","bufif1","byte","case","casex","casez","cell","chandle","class","clocking","cmos","config","const","constraint","context","continue","cover","covergroup","coverpoint","cross","deassign","default","defparam","design","disable","dist","do","edge","else","end","endcase","endclass","endclocking","endconfig","endfunction","endgenerate","endgroup","endinterface","endmodule","endpackage","endprimitive","endprogram","endproperty","endsequence","endspecify","endtable","endtask","enum","event","expect","export","extends","extern","final","first_match","for","force","foreach","forever","fork","forkjoin","function","generate","genvar","highz0","highz1","if","iff","ifnone","ignore_bins","illegal_bins","import","incdir","include","initial","inout","input","inside","instance","int","integer","interface","intersect","join","join_any","join_none","large","liblist","library","local","localparam","logic","longint","macromodule","matches","medium","modport","module","nand","negedge","new","nmos","nor","noshowcancelled","not","notif0","notif1","null","or","output","package","packed","parameter","pmos","posedge","primitive","priority","program","property","protected","pull0","pull1","pulldown","pullup","pulsestyle_ondetect","pulsestyle_onevent","pure","rand","randc","randcase","randsequence","rcmos","real","realtime","ref","reg","release","repeat","return","rnmos","rpmos","rtran","rtranif0","rtranif1","scalared","sequence","shortint","shortreal","showcancelled","signed","small","solve","specify","specparam","static","string","strong0","strong1","struct","super","supply0","supply1","table","tagged","task","this","throughout","time","timeprecision","timeunit","tran","tranif0","tranif1","tri","tri0","tri1","triand","trior","trireg","type","typedef","union","unique","unsigned","use","uwire","var","vectored","virtual","void","wait","wait_order","wand","weak0","weak1","while","wildcard","wire","with","within","wor","xnor","xor"]
                const name = d[0] + d[1].join('');
                if (keywords.includes(name)) {
                    return reject;
                } else {
                    return  {Name: name, Location: l};
                }
            }
            },
        {"name": "_$ebnf$1", "symbols": []},
        {"name": "_$ebnf$1", "symbols": ["_$ebnf$1", /[\s]/], "postprocess": function arrpush(d) {return d[0].concat([d[1]]);}},
        {"name": "_", "symbols": ["_$ebnf$1"]},
        {"name": "__$ebnf$1", "symbols": [/[\s]/]},
        {"name": "__$ebnf$1", "symbols": ["__$ebnf$1", /[\s]/], "postprocess": function arrpush(d) {return d[0].concat([d[1]]);}},
        {"name": "__", "symbols": ["__$ebnf$1"]}
    ]
      , ParserStart: "PROGRAM"
    }
    if (typeof module !== 'undefined'&& typeof module.exports !== 'undefined') {
       module.exports = grammar;
    } else {
       window.grammar = grammar;
    }
    })();
    