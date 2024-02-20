using Microsoft.Extensions.DependencyModel;
using NerdStore.Core.DomainObjects;
using System;
using System.Linq;
using Xunit;
namespace NerdStore.Vendas.Domain.Tests
{
    public class PedidoTests
    {
        private readonly Guid _produtoTesteId = Guid.NewGuid();
        private readonly Pedido _pedido;
        public PedidoTests()
        {
            _pedido = Pedido.PedidoFactory.NovoPedidoRascunho(Guid.NewGuid());
        }

        [Fact(DisplayName = "Adicionar Item Novo Pedido")]
        [Trait("Categoria", "Vendas - Pedido")]
        public void AdicionarItemPedido_NovoPedido_DeveAtualizarValor()
        {
            // Arrange
            var pedido = Pedido.PedidoFactory.NovoPedidoRascunho(Guid.NewGuid());
            var pedidoItem = new PedidoItem(Guid.NewGuid(), "Produto Teste", 2, 100);

            // Act
            pedido.AdicionarItem(pedidoItem);

            // Assert
            Assert.Equal(200, pedido.ValorTotal);
        }

        [Fact(DisplayName = "Adicionar Item Pedido Existente")]
        [Trait("Categoria", "Vendas - Pedido")]
        public void AdicionarItemPedido_ItemExistente_DeveIncrementarUnidadesSomarValores()
        {
            // Arrange

            _pedido.AdicionarItem(new PedidoItem(_produtoTesteId, "Produto Teste", 2, 100));
            var pedidoItem = new PedidoItem(_produtoTesteId, "Produto Teste", 1, 100);

            // Act
            _pedido.AdicionarItem(pedidoItem);

            // Assert
            Assert.Equal(300, _pedido.ValorTotal);
            Assert.Equal(1, _pedido.PedidoItens.Count);
            Assert.Equal(3, _pedido.PedidoItens.FirstOrDefault(p => p.ProdutoId == _produtoTesteId).Quantidade);
        }

        [Fact(DisplayName = "Adicionar Novo Item Pedido Acima do permitido")]
        [Trait("Categoria", "Vendas - Pedido")]
        public void AdicionarItemPedido_UnidadesNovoItemAcimaDoPermitido_DeveRetornarException()
        {
            // Arrange
            var produtoId = Guid.NewGuid();
            var pedidoItem = new PedidoItem(produtoId, "ProdutoTeste", Pedido.MAX_UNIDADES_ITEM + 1, 100);

            // Act & Assert
            Assert.Throws<DomainException>(() => _pedido.AdicionarItem(pedidoItem));
        }

        [Fact(DisplayName = "Adicionar Item Pedido Existente Acima do permitido")]
        [Trait("Categoria", "Vendas - Pedido")]
        public void AdicionarItemPedido_ItemExisteSomaUnidadesAcimaDoPermitido_DeveRetornarException()
        {
            // Arrange
            var produtoId = Guid.NewGuid();
            var pedidoItem = new PedidoItem(produtoId, "ProdutoTeste", Pedido.MAX_UNIDADES_ITEM, 100);
            var pedidoItem2 = new PedidoItem(produtoId, "ProdutoTeste", 1, 100);
            _pedido.AdicionarItem(pedidoItem);

            // Act & Assert
            Assert.Throws<DomainException>(() => _pedido.AdicionarItem(pedidoItem2));
        }

        [Fact(DisplayName = "Atualizar Item Pedido Inexistente")]
        [Trait("Categoria", "Vendas - Pedido")]
        public void AtualizarItemPedido_ItemNaoExisteNaLista_DeveRetornarException()
        {
            // Arrange
            var pedidoItemAtualizado = new PedidoItem(Guid.NewGuid(), "Produto Teste", 5, 100);

            // Act & Assert
            Assert.Throws<DomainException>(() => _pedido.AtualizarItem(pedidoItemAtualizado));
        }

        [Fact(DisplayName = "Atualizar Item Pedido Valido")]
        [Trait("Categoria", "Vendas - Pedido")]
        public void AtualizarItemPedido_ItemValido_DeveAtualizarQuantidade()
        {
            // Arrange
            var produtoId = Guid.NewGuid();
            _pedido.AdicionarItem(new PedidoItem(produtoId, "Produto Teste", 2, 100));
            var novaQuantidade = 5;
            var pedidoItemAtualizado = new PedidoItem(produtoId, "Produto Teste", novaQuantidade, 100);

            // Act
            _pedido.AtualizarItem(pedidoItemAtualizado);

            // Assert
            Assert.Equal(novaQuantidade, _pedido.PedidoItens.FirstOrDefault(p => p.ProdutoId == produtoId).Quantidade);
        }

        [Fact(DisplayName = "Atualizar Item Pedido Validar Total")]
        [Trait("Categoria", "Vendas - Pedido")]
        public void AtualizarItemPedido_PedidoComProdutosDiferentes_DeveAtualizarValorTotal()
        {
            // Arrange
            var produtoId = Guid.NewGuid();
            var pedidoItemExistente1 = new PedidoItem(Guid.NewGuid(), "Produto Raudo", 3, 50);
            _pedido.AdicionarItem(pedidoItemExistente1);
            var pedidoItemExistente2 = new PedidoItem(produtoId, "Produto Teste", 2, 100);
            _pedido.AdicionarItem(pedidoItemExistente2);
            
            var pedidoItemAtualizado = new PedidoItem(produtoId, "Produto Teste", 5, 100);
            var totalPedido = pedidoItemExistente1.ValorUnitario * pedidoItemExistente1.Quantidade + pedidoItemAtualizado.ValorUnitario * pedidoItemAtualizado.Quantidade;

            // Act
            _pedido.AtualizarItem(pedidoItemAtualizado);

            // Assert 
            Assert.Equal(totalPedido, _pedido.ValorTotal);
        }

        [Fact(DisplayName = "Atualizar Item Pedido Quantidade acima do permitido")]
        [Trait("Categoria", "Vendas - Pedido")]
        public void AtualizarItemPedido_ItemUnidadesAcimaDoPermitido_DeveRetornarException()
        {
            // Arrange
            var produtoId = Guid.NewGuid();
            var pedidoItemExistente1 = new PedidoItem(Guid.NewGuid(), "Produto Raudo", 3, 50);
            _pedido.AdicionarItem(pedidoItemExistente1);
            var pedidoItemExistente2 = new PedidoItem(produtoId, "Produto Teste", 2, 100);
            _pedido.AdicionarItem(pedidoItemExistente2);

            var pedidoItemAtualizado = new PedidoItem(produtoId, "Produto Teste", Pedido.MAX_UNIDADES_ITEM + 1, 100);

            // Act & Assert
            Assert.Throws<DomainException>(() => _pedido.AtualizarItem(pedidoItemAtualizado));
        }

        [Fact(DisplayName = "Remover Item Pedido Quantidade acima do permitido")]
        [Trait("Categoria", "Vendas - Pedido")]
        public void RemoverItemPedido_ItemInexistenteNaLista_DeveRetornarException()
        {
            // Arrange
            var produtoId = Guid.NewGuid();
            var pedidoItemExistente1 = new PedidoItem(Guid.NewGuid(), "Produto Raudo", 3, 50);

            // Act & Assert
            Assert.Throws<DomainException>(() => _pedido.RemoverItem(pedidoItemAtualizado));
        }
    }
}