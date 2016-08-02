using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace flushfillsrc
{
    /// <summary>
    /// Variations in function signatures:
    ///     Each has a name -- before parenthesis
    ///     Each argument is separated by a comma.
    ///     But sometimes, the comma is within paired bracked. 
    ///         func(arg1, arg2, [arg3, arg4])
    ///         This happens when arguments are expected in pairs.
    ///     And sometimes the arguments end with ellipses, if indefinite numbers are accepted.
    ///     If paired arguments are accepted, then ellipses might happen with bracket all in one comma.
    ///         func(arg1, arg2,...[arg3, arg4])
    ///         Ellipses might either be three periods or a single ellipsis character.
    ///     Spaces are meaningless.
    /// </summary>
    class Function
    {
    }
}
