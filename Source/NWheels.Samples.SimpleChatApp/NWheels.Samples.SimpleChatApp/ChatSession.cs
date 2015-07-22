using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Samples.SimpleChatApp
{
    internal class ChatSession
    {
        public long Id { get; set; }
        private HashSet<long> Participants { get; set; }

        public ChatSession()
        {
            Participants = new HashSet<long>();
        }

        public void AddParticipants(IEnumerable<long> participants)
        {
            lock ( Participants )
            {
                Participants.IntersectWith(participants);
            }
        }

        public void RemoveParticipants(IEnumerable<long> participants)
        {
            lock ( Participants )
            {
                foreach ( long participant in participants )
                {
                    Participants.Remove(participant);
                }
            }
        }
    }
}
