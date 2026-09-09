using InterfacesComProposito.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InterfacesComProposito.Infrastructure.Data.Configurations;

public sealed class PagamentoConfiguration : IEntityTypeConfiguration<Pagamento>
{
    public void Configure(EntityTypeBuilder<Pagamento> builder)
    {
        builder.ToTable("Pagamentos");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.PedidoId)
            .IsRequired();

        builder.Property(p => p.Valor)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(p => p.Status)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(p => p.CriadoEm)
            .IsRequired();

        builder.Property(p => p.ProcessadoEm)
            .IsRequired(false);
    }
}
