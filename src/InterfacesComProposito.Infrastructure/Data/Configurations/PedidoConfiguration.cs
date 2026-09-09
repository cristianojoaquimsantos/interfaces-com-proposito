using InterfacesComProposito.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InterfacesComProposito.Infrastructure.Data.Configurations;

public sealed class PedidoConfiguration : IEntityTypeConfiguration<Pedido>
{
    public void Configure(EntityTypeBuilder<Pedido> builder)
    {
        builder.ToTable("Pedidos");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.UsuarioId)
            .IsRequired();

        builder.Property(p => p.Status)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(p => p.ValorTotal)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(p => p.CriadoEm)
            .IsRequired();

        builder.Property(p => p.FinalizadoEm)
            .IsRequired(false);

        builder.Property(p => p.ComprovanteUrl)
            .HasMaxLength(500)
            .IsRequired(false);

        builder.HasMany(p => p.Itens)
            .WithOne()
            .HasForeignKey(i => i.PedidoId)
            .OnDelete(DeleteBehavior.Cascade);

        // Garante que o EF Core acesse a coleção interna via backing field _itens
        builder.Navigation(p => p.Itens)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
