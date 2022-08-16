grammar SearchString;

line    :   part+ ;
part    :   text                # TextWithoutField
        |   field ':' text      # TextWithField
        ;

field   :   WORD ;
text    :   WORD                # OneWord
        |   MANYWORDS           # SeveralWords
        ;

WORD            :   ~[ ":\n\r]+ ;
MANYWORDS       :   '"' ~["]+ '"';
WS              :   [ \t\n\r]+ -> skip ;