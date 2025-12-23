// ***********************************************************************
// $Library:     eonic.dbhelper
// $Revision:    3.1  
// $Date:        2006-03-02
// $Author:      Trevor Spink (trevor@eonic.co.uk)
// &Website:     www.eonic.co.uk
// &Licence:     All Rights Reserved.
// $Copyright:   Copyright (c) 2002 - 2024 Trevor Spink Consultants Ltd.
// ***********************************************************************



using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace Protean
{



    public partial class Cms
    {

        // Inherits dbTools
      
            // Inherits dbTools
            public partial class dbHelper : Tools.Database
            {
                /// <summary>
                /// Asynchronously gets the layout for a specific page.
                /// If the page is cloned, retrieves the layout from the source page.
                /// </summary>
                /// <param name="nPageId">Page ID to get layout for</param>
                /// <param name="cancellationToken">Cancellation token</param>
                /// <returns>Layout name or "default"</returns>
                public async Task<string> getPageLayoutAsync(
                    long nPageId,
                    CancellationToken cancellationToken = default)
                {
                    PerfMonLog("DBHelper", "getPageLayoutAsync");

                    string cLayout = "";
                    string cSql = "";

                    try
                    {
                        // ✅ FIXED: Check for cloned pages first (same as sync version)
                        if (Cms.gbClone)
                        {
                            // If the page is cloned then we need to look at the page that it's cloned from
                            cSql = "SELECT nCloneStructId FROM tblContentStructure WHERE nStructKey = " + nPageId;

                            using (var reader = await getDataReaderDisposableAsync(cSql, CommandType.Text, null, cancellationToken).ConfigureAwait(false))
                            {
                                if (reader != null && await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
                                {
                                    if (!reader.IsDBNull(0))
                                    {
                                        long nClonePageId = reader.GetInt64(0);
                                        if (nClonePageId > 0L)
                                            nPageId = nClonePageId;
                                    }
                                }
                            }
                        }

                        // ✅ FIXED: Query the correct table (tblContentStructure, not tblDirectory)
                        cSql = "SELECT cStructLayout FROM tblContentStructure WHERE nStructKey = " + nPageId;

                        using (var reader = await getDataReaderDisposableAsync(cSql, CommandType.Text, null, cancellationToken).ConfigureAwait(false))
                        {
                            if (reader != null && await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
                            {
                                if (!reader.IsDBNull(0))
                                {
                                    cLayout = reader.GetString(0);
                                }
                            }
                        }

                        // Return "default" if no layout found
                        return string.IsNullOrEmpty(cLayout) ? "default" : cLayout;
                    }
                    catch (System.Exception ex)
                    {
                        OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(
                            mcModuleName, "getPageLayoutAsync", ex, cSql));
                        return "default";
                    }
                }
            }

    }
}
