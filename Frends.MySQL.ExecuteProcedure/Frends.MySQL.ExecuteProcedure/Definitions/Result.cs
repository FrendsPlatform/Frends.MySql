namespace Frends.MySQL.ExecuteProcedure.Definitions
{
    public class Result
    {
        public int AffectedRows { get; private set; }

        public Result(int affectedRows)
        {
            AffectedRows = affectedRows;
        }
    }
}
