#region Copyright © 2015 Denis Pavlichenko. All rights reserved.
/*
Copyright © 2015 Denis Pavlichenko. All rights reserved.

Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions
are met:

1. Redistributions of source code must retain the above copyright
   notice, this list of conditions and the following disclaimer.
2. Redistributions in binary form must reproduce the above copyright
   notice, this list of conditions and the following disclaimer in the
   documentation and/or other materials provided with the distribution.
3. The name of the author may not be used to endorse or promote products
   derived from this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE AUTHOR "AS IS" AND ANY EXPRESS OR
IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES
OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED.
IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR ANY DIRECT, INDIRECT,
INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT
NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY
THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE. 
*/
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using System.Text.RegularExpressions;
using System.IO;

namespace MSBuild.Community.Tasks
{
	/// <summary>
	/// Inject License/Copyright banner in source files from a boilerplate.
	/// </summary>
	/// <example>Add license header at the top of multiple JS files if previous boilerplate not found.
	/// <code><![CDATA[
	/// <ItemGroup>
	///		<jsFiles Include="files\**\*.js" />
	///		<bpJs Include="licHead.txt" />
	///	</ItemGroup>
	///	<Brand Files="%(jsFiles.FullPath)"
	///     SkipWhenHas="Copyright \(c\)"
	///     FromBoilerplate="@(bpJs)"
	///		InsertAtTheTop="true"
	/// ]]></code>
	/// </example>
    public class Brand : Task
    {
        /// <summary>
        /// Initializes a new instance of the Brand class.
        /// </summary>
        public Brand()
        {
            InsertAtTheTop = true;
            SkipWhenHas = "";
            Files = null;
            FromBoilerplate = null;
        }

        #region Properties

        /// <summary>
        /// Gets or sets the files to update.
        /// </summary>
        /// <value>Multiple file paths.</value>
        public ITaskItem[] Files { get; set; }

        /// <summary>
        /// Gets or sets the boilerplate file path which contains the license text.
        /// </summary>
        /// <value>A path to file.</value>
        public ITaskItem FromBoilerplate { get; set; }

        /// <summary>
        /// Gets or sets regex condition upon which the file should be skipped. 
		///	Optional. If omitted, the boilerplate is only injected when the file does not already contain the full boilerplate text.
        /// </summary>
        /// <value>A regular expression string.</value>
        public string SkipWhenHas { get; set; }

        /// <summary>
        /// Gets or sets whether the boilerplate should be added at the top or else at the bottom of the file being updated.
        /// </summary>
        /// <value>True or False.</value>
        public bool InsertAtTheTop { get; set; }

        #endregion

        /// <summary>
        /// The Execute method.
        /// </summary>
        /// <remarks>
		///	This will execute the task. 
		///	The task will log which files got updated, otherwise nothing is returned to the caller.
        ///	An optional $(Year) wildcard in the boilerplate will automatically be replaced with current year.
		///	</remarks>
        /// <returns>
        /// true if the task successfully executed; otherwise, false.
        /// </returns>
        public override bool Execute()
        {
            try
            {
                if (Files == null || Files.Length == 0)
                {
                    Log.LogMessage("Brand task completed, no input files specified.");
                    return true;
                }

                if (FromBoilerplate == null)
                {
                    Log.LogMessage("Brand task completed, no boilerplate specified.");
                    return true;
                }

                string boilerPlate = File
                    .ReadAllText(FromBoilerplate.ToString())
                    .Replace("$(Year)", DateTime.Now.Year.ToString())
                    ;

                if (string.IsNullOrWhiteSpace(boilerPlate))
                {
                    Log.LogMessage("Brand task completed, boilerplate empty.");
                    return true;
                }

                if (string.IsNullOrWhiteSpace(SkipWhenHas))
                {
                    SkipWhenHas = boilerPlate;
                }
                
                Regex regexSkipWhenHas = new Regex(SkipWhenHas, RegexOptions.IgnoreCase);

                foreach (ITaskItem item in Files)
                {
                    string fileName = item.ItemSpec;
                    string buffer = File.ReadAllText(fileName);

                    if (!regexSkipWhenHas.IsMatch(buffer))
                    {
                        if (InsertAtTheTop)
                        {
                            buffer = boilerPlate + buffer;
                        }
                        else
                        {
                            buffer += boilerPlate;
                        }
                        File.WriteAllText(fileName, buffer);
                        Log.LogMessage("Branded File \"{0}\".", fileName);
                    }
                    /*else
                    {
                        Log.LogMessage("Skipped File \"{0}\".", fileName);
                    }*/
                }
            }

            catch (Exception ex)
            {
                Log.LogErrorFromException(ex, true, true, null);
                return false;
            } 
            
            return true;
        }
    }
}
