using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace M_21_31.Logger
{
    public enum EventTypes
    {
        None = 0,
        [Description("Event Executed")]
        EventExecuted = 101,
        [Description("Information")]
        Information = 200,
        [Description("Debug")]
        Debug = 201,
        [Description("Warning")]
        Warning = 202,
        [Description("Error")]
        Error = 203,
        [Description("Critical")]
        Critical = 204,
        [Description("User Authentication")]
        UserAuthentication = 102,
        [Description("Request")]
        Request = 103,
        [Description("Unhandled Exception")]
        UnhandledException = 104,
        [Description("Database Command Executed")]
        DatabaseCommandExecuted = 105,
        [Description("Page Accessed")]
        PageAccessed = 106,
        [Description("Function Executed")]
        FunctionExecuted = 107,
        [Description("Email Sent")]
        EmailSent = 108,
        [Description("File Accessed")]
        FileTransfer = 109,
        [Description("File Added")]
        FileAdded = 110,
        [Description("File Downloaded")]
        FileDownload = 111,
        [Description("File Deleted")]
        FileDeleted = 112,
        [Description("System Status")]
        SystemStatus = 113,
        [Description("System Access")]
        SystemAccess = 114,
        [Description("System Configuration Change")]
        SystemConfigurationChange = 115,
        [Description("User Added")]
        UserAdded = 116,
        [Description("User Removed")]
        UserRemoved = 117,
        [Description("User Configuration Change")]
        UserConfigurationChange = 118,
        [Description("Response")]
        Response = 119,

    }
}
