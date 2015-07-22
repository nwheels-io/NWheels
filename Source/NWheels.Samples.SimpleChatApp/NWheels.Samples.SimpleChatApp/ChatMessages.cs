using System;
using System.Collections.Generic;

namespace NWheels.Samples.SimpleChatApp
{
    public enum LoginErrorCode
    {
        Success = 0,
        BadUsernameOrPwd = 1,
        InternalError = 2
    }

    class ChatMessages
    {
        //-------------------------------------------------------------------------------------------
        // Clients ==> Server
        //-------------------------------------------------------------------------------------------
        
        public class LoginRequest
        {
            public string Username { get; set; }
            public string Password { get; set; }
        }

        public class SendMessageNotInChatSession
        {
            public long TargetUserId { get; set; }

            public string MessageContent { get; set; }

            public DateTime SendTimeUtc { get; set; }
        }

        public class StartNewChatSessionRequest
        {
            public List<long> Participants { get; set; }

            public long ClientChatSessionId { get; set; }
        }

        public class ChangeUsersChatSessionRequest
        {
            public long ServerChatSessionId { get; set; }
            public List<long> NewParticipants { get; set; }
            public List<long> RemoveParticipants { get; set; }
        }



        //-------------------------------------------------------------------------------------------
        // Server ==> Clients
        //-------------------------------------------------------------------------------------------

        public class LoginResponse
        {
            public LoginErrorCode Result { get; set; }
            public string Username { get; set; }            // The user name as found in the DB
            public long UserId { get; set; }
        }

        public class ForwardMessage
        {
            public long SourceUserId { get; set; }

            public string MessageContent { get; set; }

            public DateTime SendTimeUtc { get; set; }

            public long ServerChatId { get; set; }  // 0 = no chat session
        }

        public class StartNewChatSessionResponse
        {
            public long ClientChatId { get; set; }
            public long ServerChatId { get; set; }
        }

        public class NewChatSessionStarted
        {
            public long ServerChatId { get; set; }
            public List<long> Participants { get; set; }
        }

        public class UpdateChatSessionParticipantsResponse
        {
            public long ChatId { get; set; }
            public List<long> NewParticipants { get; set; }
            public List<long> RemoveParticipants { get; set; }

        }

        //-------------------------------------------------------------------------------------------

    }
}
