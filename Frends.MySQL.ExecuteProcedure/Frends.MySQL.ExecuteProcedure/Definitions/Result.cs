namespace Frends.MySQL.ExecuteProcedure.Definitions
{
    public class Result
    {
        /// <summary>
        /// Count of affected rows
        /// </summary>
        /// <example>10</example>
        public int AffectedRows { get; private set; }

        internal Result(int affectedRows)
        {
            AffectedRows = affectedRows;
        }
    }
}
