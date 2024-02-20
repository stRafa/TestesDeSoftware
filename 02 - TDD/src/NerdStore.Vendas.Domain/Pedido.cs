using NerdStore.Core.DomainObjects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NerdStore.Vendas.Domain
{
    public class Pedido
    {
        public static int MAX_UNIDADES_ITEM => 15;
        public static int MIN_UNIDADES_ITEM => 1;

        protected Pedido()
        {
            _pedidoItens = new List<PedidoItem>();
        }

        public Guid ClienteId { get; private set; }
        public decimal ValorTotal { get; private set; }
        public PedidoStatus PedidoStatus { get; private set; }

        private readonly List<PedidoItem> _pedidoItens;
        public IReadOnlyCollection<PedidoItem> PedidoItens => _pedidoItens;

        public void AdicionarItem(PedidoItem pedidoItem)
        {
            var itemExistente = _pedidoItens.FirstOrDefault(p => p.ProdutoId == pedidoItem.ProdutoId);
            if (itemExistente != null)
            {
                itemExistente.AdicionarUnidades(pedidoItem.Quantidade);
                ValidarQuantidadeItemPermitida(itemExistente);
            }
            else
            {
                ValidarQuantidadeItemPermitida(pedidoItem);
                _pedidoItens.Add(pedidoItem);
            }


            CalcularValorPedido();
        }

        public void AtualizarItem(PedidoItem pedidoItem)
        {
            ValidarPedidoItemInexistente(pedidoItem);
            ValidarQuantidadeItemPermitida(pedidoItem);

            var pedidoItemExistente = _pedidoItens.FirstOrDefault(p => p.ProdutoId == pedidoItem.ProdutoId);

            _pedidoItens.Remove(pedidoItemExistente);
            _pedidoItens.Add(pedidoItem);

            CalcularValorPedido();
        }


        private void CalcularValorPedido()
        {
            ValorTotal = _pedidoItens.Sum(p => p.CalcularValor());
        }

        public void TornarRascunho()
        {
            PedidoStatus = PedidoStatus.Rascunho;
        }

        private void ValidarPedidoItemInexistente(PedidoItem pedidoItem)
        {
            if(_pedidoItens.FirstOrDefault(p => p.ProdutoId == pedidoItem.ProdutoId) is null) throw new DomainException("O item não existe no pedido");
        }

        private void ValidarQuantidadeItemPermitida(PedidoItem item)
        {
            if (item.Quantidade > MAX_UNIDADES_ITEM)
                throw new DomainException($"Máximo de {MAX_UNIDADES_ITEM} unidades por produto");
            
            if (item.Quantidade < MIN_UNIDADES_ITEM)
                throw new DomainException($"Mínimo de {MIN_UNIDADES_ITEM} unidades por produto");
        }

        public static class PedidoFactory
        {
            public static Pedido NovoPedidoRascunho(Guid clienteId)
            {
                var pedido = new Pedido
                {
                    ClienteId = clienteId
                };

                pedido.TornarRascunho();
                return pedido;
            }
        }
    }


    public enum PedidoStatus
    {
        Rascunho = 0,
        Iniciado = 1,
        Pago = 4,
        Entregue = 5,
        Cancelado = 6
    }


}