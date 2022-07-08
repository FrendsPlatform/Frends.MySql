using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frends.MySQL.ExecuteProcedure.Definitions;
public class Result
{
    public int AffectedRows { get; private set; }

    public Result(int affectedRows) { 
        AffectedRows = affectedRows;
    }
}
