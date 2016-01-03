namespace Locutor_da_Hora
{
    /// <summary>
    /// Define valores para campos comuns entre classes da aplicação. Serve como referência para todos os usos destes campos.
    /// </summary>
    public sealed class Contract
    {
        public sealed class Analytics
        {
            public static readonly string APLICACAO = "Aplicação";
            public static readonly string INTERACOES = "Interações";

            public sealed class Aplicacao
            {
                public static readonly string INICIALIZACAO = "Inicialização";
                public static readonly string FINALIZACAO = "Finalização";
            }

            public sealed class Interacoes
            {            
                public static readonly string AVALIAR = "Avaliar";
                public static readonly string GRAVACAO = "Gravação";
                public static readonly string CRIAR_LOCUCAO = "Criar Locução";
                public static readonly string EDITAR_LOCUCAO = "Editar Locução";
                public static readonly string EXCLUIR_LOCUCAO = "Excluir Locução";
            }
        }
    }
}
