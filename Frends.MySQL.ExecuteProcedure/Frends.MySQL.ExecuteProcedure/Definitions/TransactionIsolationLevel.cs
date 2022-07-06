using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frends.MySQL.ExecuteProcedure.Definitions;
public enum TransactionIsolationLevel
{
    Default,
    ReadCommitted,
    Serializable,
    ReadUncommitted,
    RepeatableRead
}
