/* created on 5/12/2015 5:43:56 PM from peg generator V1.0 using 'GrammarUnderTest.txt' as input*/

using System;
using System.IO;

namespace NWheels.Tools.DevFlow.Parsers
{
    enum VsSlnNodeType
    {
        S = 23, EOL = 24, NOT_EOL = 25, SKIP_TO_EOL = 26, INDENT = 27, EOF = 28,
        QUOTE = 29, NOT_QUOTE = 30, COMMA = 31, solution = 1, header_line = 2,
        project = 3, project_section = 4, project_section_line = 5, global = 6,
        global_section = 7, global_section_header = 8, pre_post_solution = 9,
        pre_solution = 10, post_solution = 11, global_section_line = 12,
        global_section_id = 13, lhs_guid_line = 14, lhs_guid_line_rhs = 15,
        lhs_rhs_guid_line = 16, guid = 18, quoted_string = 19, @string = 20,
        quoted_guid = 21, project_header = 22
    };
    class VisualStudioSolutionParser : PegCharParser
    {

        #region Input Properties
        public static EncodingClass encodingClass = EncodingClass.ascii;
        public static UnicodeDetection unicodeDetection = UnicodeDetection.notApplicable;
        #endregion Input Properties
        #region Constructors
        public VisualStudioSolutionParser()
            : base()
        {

        }
        public VisualStudioSolutionParser(string src, TextWriter FerrOut)
            : base(src, FerrOut)
        {

        }
        #endregion Constructors
        #region Overrides
        public override string GetRuleNameFromId(int id)
        {
            try
            {
                VsSlnNodeType ruleEnum = (VsSlnNodeType)id;
                string s = ruleEnum.ToString();
                int val;
                if (int.TryParse(s, out val))
                {
                    return base.GetRuleNameFromId(id);
                }
                else
                {
                    return s;
                }
            }
            catch (Exception)
            {
                return base.GetRuleNameFromId(id);
            }
        }
        public override void GetProperties(out EncodingClass encoding, out UnicodeDetection detection)
        {
            encoding = encodingClass;
            detection = unicodeDetection;
        }
        #endregion Overrides
        #region Grammar Rules
        public bool S()    /*S: [ \t]+ ;*/
        {

            return PlusRepeat(() => OneOf(" \t"));
        }
        public bool EOL()    /*EOL: [\r\n]+ ;*/
        {

            return PlusRepeat(() => OneOf("\r\n"));
        }
        public bool NOT_EOL()    /*NOT_EOL: ![\r\n] ;*/
        {

            return Not(() => OneOf("\r\n"));
        }
        public bool SKIP_TO_EOL()    /*SKIP_TO_EOL: (NOT_EOL.)* EOL ;*/
        {

            return And(() =>
                      OptRepeat(() => And(() => NOT_EOL() && Any()))
                   && EOL());
        }
        public bool INDENT()    /*INDENT: [ \t]+ ;*/
        {

            return PlusRepeat(() => OneOf(" \t"));
        }
        public bool EOF()    /*EOF: !. ;*/
        {

            return Not(() => Any());
        }
        public bool QUOTE()    /*QUOTE: '"' ;*/
        {

            return Char('"');
        }
        public bool NOT_QUOTE()    /*NOT_QUOTE: !'"' ;*/
        {

            return Not(() => Char('"'));
        }
        public bool COMMA()    /*COMMA: S? ',' S? ;*/
        {

            return And(() =>
                      Option(() => S())
                   && Char(',')
                   && Option(() => S()));
        }
        public bool solution()    /*[1] ^^solution: @(header_line+) @(project+) @global ;*/
        {

            return TreeNT((int)VsSlnNodeType.solution, () =>
                 And(() =>
                      (
                          PlusRepeat(() => header_line())
                       || Fatal("<<(header_line+)>> expected"))
                   && (
                          PlusRepeat(() => project())
                       || Fatal("<<(project+)>> expected"))
                   && (global() || Fatal("<<global>> expected"))));
        }
        public bool header_line()    /*[2] ^^header_line: !'Project(' SKIP_TO_EOL ;*/
        {

            return TreeNT((int)VsSlnNodeType.header_line, () =>
                 And(() => Not(() => Char("Project(")) && SKIP_TO_EOL()));
        }
        public bool project()    /*[3] ^^project: project_header project_section* 'EndProject' EOL ;*/
        {

            return TreeNT((int)VsSlnNodeType.project, () =>
                 And(() =>
                      project_header()
                   && OptRepeat(() => project_section())
                   && Char("EndProject")
                   && EOL()));
        }
        public bool project_section()    /*[4] ^^project_section: INDENT? 'ProjectSection' SKIP_TO_EOL project_section_line* INDENT? 'EndProjectSection' EOL ;*/
        {

            return TreeNT((int)VsSlnNodeType.project_section, () =>
                 And(() =>
                      Option(() => INDENT())
                   && Char("ProjectSection")
                   && SKIP_TO_EOL()
                   && OptRepeat(() => project_section_line())
                   && Option(() => INDENT())
                   && Char("EndProjectSection")
                   && EOL()));
        }
        public bool project_section_line()    /*[5] ^^project_section_line: INDENT? !'EndProjectSection' SKIP_TO_EOL ;*/
        {

            return TreeNT((int)VsSlnNodeType.project_section_line, () =>
                 And(() =>
                      Option(() => INDENT())
                   && Not(() => Char("EndProjectSection"))
                   && SKIP_TO_EOL()));
        }
        public bool global()    /*[6] ^^global: 'Global' EOL global_section* 'EndGlobal' EOL @EOF ;*/
        {

            return TreeNT((int)VsSlnNodeType.global, () =>
                 And(() =>
                      Char('G', 'l', 'o', 'b', 'a', 'l')
                   && EOL()
                   && OptRepeat(() => global_section())
                   && Char("EndGlobal")
                   && EOL()
                   && (EOF() || Fatal("<<EOF>> expected"))));
        }
        public bool global_section()    /*[7] ^^global_section: global_section_header global_section_line* INDENT? 'EndGlobalSection' EOL ;*/
        {

            return TreeNT((int)VsSlnNodeType.global_section, () =>
                 And(() =>
                      global_section_header()
                   && OptRepeat(() => global_section_line())
                   && Option(() => INDENT())
                   && Char("EndGlobalSection")
                   && EOL()));
        }
        public bool global_section_header()    /*[8] ^^global_section_header:
			INDENT? 'GlobalSection(' global_section_id ')' S? '=' S? pre_post_solution EOL ;*/
        {

            return TreeNT((int)VsSlnNodeType.global_section_header, () =>
                 And(() =>
                      Option(() => INDENT())
                   && Char("GlobalSection(")
                   && global_section_id()
                   && Char(')')
                   && Option(() => S())
                   && Char('=')
                   && Option(() => S())
                   && pre_post_solution()
                   && EOL()));
        }
        public bool pre_post_solution()    /*[9] ^pre_post_solution: pre_solution / post_solution ;*/
        {

            return TreeAST((int)VsSlnNodeType.pre_post_solution, () =>
                     pre_solution() || post_solution());
        }
        public bool pre_solution()    /*[10] ^^pre_solution: 'preSolution' ;*/
        {

            return TreeNT((int)VsSlnNodeType.pre_solution, () =>
                 Char("preSolution"));
        }
        public bool post_solution()    /*[11] ^^post_solution: 'postSolution' ;*/
        {

            return TreeNT((int)VsSlnNodeType.post_solution, () =>
                 Char("postSolution"));
        }
        public bool global_section_line()    /*[12] ^^global_section_line: lhs_rhs_guid_line / lhs_guid_line / (INDENT? !'EndGlobalSection' SKIP_TO_EOL) ;*/
        {

            return TreeNT((int)VsSlnNodeType.global_section_line, () =>

                      lhs_rhs_guid_line()
                   || lhs_guid_line()
                   || And(() =>
                          Option(() => INDENT())
                       && Not(() => Char("EndGlobalSection"))
                       && SKIP_TO_EOL()));
        }
        public bool global_section_id()    /*[13] ^^global_section_id: [A-Za-z0-9_]+ ;*/
        {

            return TreeNT((int)VsSlnNodeType.global_section_id, () =>
                 PlusRepeat(() =>
                   (In('A', 'Z', 'a', 'z', '0', '9') || OneOf("_"))));
        }
        public bool lhs_guid_line()    /*[14] ^^lhs_guid_line: INDENT? guid '.' lhs_guid_line_rhs ;*/
        {

            return TreeNT((int)VsSlnNodeType.lhs_guid_line, () =>
                 And(() =>
                      Option(() => INDENT())
                   && guid()
                   && Char('.')
                   && lhs_guid_line_rhs()));
        }
        public bool lhs_guid_line_rhs()    /*[15] ^^lhs_guid_line_rhs: SKIP_TO_EOL ;*/
        {

            return TreeNT((int)VsSlnNodeType.lhs_guid_line_rhs, () =>
                 SKIP_TO_EOL());
        }
        public bool lhs_rhs_guid_line()    /*[16] ^^lhs_rhs_guid_line: INDENT? guid S? '=' S? guid EOL ;*/
        {

            return TreeNT((int)VsSlnNodeType.lhs_rhs_guid_line, () =>
                 And(() =>
                      Option(() => INDENT())
                   && guid()
                   && Option(() => S())
                   && Char('=')
                   && Option(() => S())
                   && guid()
                   && EOL()));
        }
        public bool guid()    /*[18] ^^guid: '{' [A-Fa-f0-9-]{36,36} '}' ;*/
        {

            return TreeNT((int)VsSlnNodeType.guid, () =>
                 And(() =>
                      Char('{')
                   && ForRepeat(36, 36, () =>
                       (In('A', 'F', 'a', 'f', '0', '9') || OneOf("-")))
                   && Char('}')));
        }
        public bool quoted_string()    /*[19] ^quoted_string: QUOTE string QUOTE ;*/
        {

            return TreeAST((int)VsSlnNodeType.quoted_string, () =>
                 And(() => QUOTE() && @string() && QUOTE()));
        }
        public bool @string()    /*[20] ^^string: (NOT_QUOTE.)* ;*/
        {

            return TreeNT((int)VsSlnNodeType.@string, () =>
                 OptRepeat(() => And(() => NOT_QUOTE() && Any())));
        }
        public bool quoted_guid()    /*[21] ^quoted_guid: QUOTE guid QUOTE ;*/
        {

            return TreeAST((int)VsSlnNodeType.quoted_guid, () =>
                 And(() => QUOTE() && guid() && QUOTE()));
        }
        public bool project_header()    /*[22] ^^project_header: 
		'Project(' quoted_guid ')' S? '=' S? quoted_string COMMA quoted_string COMMA quoted_guid S? EOL ;*/
        {

            return TreeNT((int)VsSlnNodeType.project_header, () =>
                 And(() =>
                      Char("Project(")
                   && quoted_guid()
                   && Char(')')
                   && Option(() => S())
                   && Char('=')
                   && Option(() => S())
                   && quoted_string()
                   && COMMA()
                   && quoted_string()
                   && COMMA()
                   && quoted_guid()
                   && Option(() => S())
                   && EOL()));
        }
        #endregion Grammar Rules
    }
}