using System;
using System.Data.Linq;
using System.Transactions;

namespace Spolty.Framework.Helpers
{
    public class SubmitHelper
    {
        #region Private Constants

        private const int DEFAULT_UPDATE_OBJECTS_RETRIES_COUNT = 2;

        #endregion

        #region Public Static Methods

        public static bool SubmitOperation(DataContext context, Action dbProcessingCode, Action cleanUpCode, bool useTransaction)
        {
            return SubmitOperation(context, dbProcessingCode, useTransaction);
        }

        public static bool SubmitOperation(DataContext context, Action dbProcessingCode, bool useTransaction)
        {
            int retriesCount = 0;

            while (retriesCount < DEFAULT_UPDATE_OBJECTS_RETRIES_COUNT)
            {
                retriesCount++;

                TransactionScope ts = null;

                if (useTransaction)
                {
                    ts = new TransactionScope();
                }

                try
                {
                    dbProcessingCode();

                    try
                    {
                        context.SubmitChanges(ConflictMode.FailOnFirstConflict);

                        if (ts != null)
                        {
                            ts.Complete();
                        }

                        return true;
                    }
                    catch (Exception)
                    {
                        if (retriesCount >= DEFAULT_UPDATE_OBJECTS_RETRIES_COUNT)
                        {
                            throw;
                        }
                    }
                }
                finally
                {
                    if (ts != null)
                    {
                        ts.Dispose();
                    }
                }
            }
            return false;
        }

        #endregion
    }
}