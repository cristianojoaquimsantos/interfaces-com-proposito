using InterfacesComProposito.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InterfacesComProposito.Infrastructure.Data.Configurations;

public sealed class ItemPedidoConfiguration : IEntityTypeConfiguration<ItemPedido>
{
    public void Configure(EntityTypeBuilder<ItemPedido> builder)
    {
        builder.ToTable("ItensPedido");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.PedidoId)
            .IsRequired();

        builder.Property(i => i.ProdutoId)
            .IsRequired();

        builder.Property(i => i.NomeProduto)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(i => i.PrecoUnitario)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(i => i.Quantidade)
            .IsRequired();

        // Subtotal é uma propriedade computada no domínio, não necessita coluna persistida separadamente
        builder.Ignore(i => i.Subtotal);
    }
}
