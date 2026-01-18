using FCG.Payments.Domain.Entities.Mediatr;

namespace FCG.Payments.Domain
{
    public abstract class Entity
    {
        public Guid Id { get; set; }
        public bool IsDeleted { get; protected set; } = false;
        public DateTime CreatedAt { get; protected set; }
        public DateTime? UpdatedAt { get; protected set; }
        public DateTime? DeletedAt { get; protected set; }

        protected Entity()
        {
            Id = Guid.NewGuid();
            CreatedAt = DateTime.UtcNow;
        }

        private List<Event> _notificacoes;

        public IReadOnlyCollection<Event> Notificacoes => _notificacoes?.AsReadOnly();

        public void AddEvent(Event evento)
        {
            _notificacoes = _notificacoes ?? new List<Event>();
            _notificacoes.Add(evento);
        }

        public void RemoveEvent(Event evento)
        {
            _notificacoes?.Remove(evento);
        }

        public void ClearEvents()
        {
            _notificacoes?.Clear();
        }

        #region Comparisons
        public override bool Equals(object obj)
        {
            var compareTo = obj as Entity;

            if (ReferenceEquals(this, compareTo)) return true;
            if (ReferenceEquals(null, compareTo)) return false;

            return Id.Equals(compareTo.Id);
        }

        public override string ToString()
        {
            return $"{ GetType().Name } [Id={Id}]";
        }

        public static bool operator == (Entity a, Entity b)
        {
            if (ReferenceEquals(a, null) && ReferenceEquals(b, null))
                return true;

            if(ReferenceEquals(a, null) || ReferenceEquals(b, null)) 
                return false;

            return a.Equals(b);
        }

        public static bool operator !=(Entity a, Entity b)
        {
            return !(a == b);
        }

        public override int GetHashCode()
        {
            return GetType().GetHashCode() * 907 + Id.GetHashCode();
        }
        #endregion
    }
}
