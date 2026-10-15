using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PlantPulse.Domain.Common
{
    /// <summary>
    /// Olayları biriktiren ana nesne. Aggregate önemli bir şey yaptığında Raise ile olay üretir; Infrastructure kayıt sırasında bu olayları outbox'a taşır.
    /// </summary>
    public abstract class AggregateRoot
    {
        private readonly List<IDomainEvent> _domainEvents = new();

        /// <summary>
        /// Biriken olaylar (dışarıdan sadece okunabilir).
        /// </summary>
        public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

        /// <summary>
        /// Olay üret: listeye ekler. Sadece aggregate'in kendisi çağırabilir (protected), böylece olay her zaman gerçekten olmuş bir şeyi anlatır.
        /// </summary>
        protected void Raise(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);
        /// <summary>
        /// Biriken olayları temizler. Olaylar outbox'a yazıldıktan sonra çağrılır.
        /// </summary>
        public void ClearDomainEvents() => _domainEvents.Clear();
    }
}