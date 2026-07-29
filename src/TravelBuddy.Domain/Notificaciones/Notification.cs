using System;
using Volo.Abp.Domain.Entities.Auditing;

namespace TravelBuddy.Notificaciones
{
    public class Notification : CreationAuditedEntity<Guid>
    {
        public Guid UserId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public bool IsRead { get; set; }

        protected Notification() { }

        public Notification(Guid id, Guid userId, string title, string message) : base(id)
        {
            UserId = userId;
            Title = title;
            Message = message;
            IsRead = false;
        }

        public void MarkAsRead()
        {
            IsRead = true;
        }
    }
}
