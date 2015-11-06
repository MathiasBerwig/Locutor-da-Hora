using System;
using System.Globalization;
using System.Text;
using System.Windows.Data;
using Locutor_da_Hora.Model;
using Locutor_da_Hora.Pages;

namespace Locutor_da_Hora.Converters
{
    class TagsToValuesTextConverter : IValueConverter
    {
        #region IValueConverter
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            StringBuilder sb = new StringBuilder(value as String);
            Usuario us = Identificacao.Instance.Usuario;
            // Nome
            if (us != null)
            {
                if (us.Nome != null)
                    sb.Replace("<nome>", us.Nome.Trim());
                // Nome da Rádio/Instituição
                if (us.Radio != null)
                    sb.Replace("<radio>", us.Radio.Trim());
                // Cidade
                if (us.Cidade != null)
                    sb.Replace("<cidade>", us.Cidade.Trim());
                // UF
                if (us.Uf != null)
                    sb.Replace("<uf>", us.Uf);
                // Estado
                sb.Replace("<estado>", RetornarEstado(us.Uf));
                // Região
                sb.Replace("<regiao>", RetornarRegiao(us.Uf));
            }
            // Dia da Semana
            sb.Replace("<dia_semana>", DateTime.Today.ToString("dddd"));
            // Horário
            sb.Replace("<hora_minuto>", HoraPorExtenso(DateTime.Now.Hour, DateTime.Now.Minute));
            // Cumprimento
            if (DateTime.Now.Hour >= 06 & DateTime.Now.Hour < 12)
            { sb.Replace("<cumprimento>", "Bom dia"); }
            else
            if (DateTime.Now.Hour >= 12 & DateTime.Now.Hour < 19)
            { sb.Replace("<cumprimento>", "Boa tarde"); }
            else
            if (DateTime.Now.Hour >= 19 || DateTime.Now.Hour < 06)
            { sb.Replace("<cumprimento>", "Boa noite"); }

            return sb.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region Métodos Auxiliares
        /// <summary>
        /// Retorna a região do país a qual pertence a Unidade Federativa (UF) informada.
        /// </summary>
        /// <param name="UF"></param>
        /// <returns></returns>
        private static string RetornarRegiao(string UF)
        {
            switch (UF)
            {
                case "PR":
                case "SC":
                case "RS": return "Sul";

                case "MT":
                case "MS":
                case "DF":
                case "GO": return "Centro-Oeste";

                case "RJ":
                case "ES":
                case "MG":
                case "SP": return "Sudeste";

                case "AC":
                case "PA":
                case "AP":
                case "AM":
                case "RO":
                case "RR":
                case "TO": return "Norte";

                case "BA":
                case "MA":
                case "CE":
                case "PB":
                case "PE":
                case "PI":
                case "RN":
                case "AL":
                case "SE": return "Nordeste";
            }
            return "Estado Inválido!";
        }

        /// <summary>
        /// Retorna o horário por extenso de acordo com os números informatos.
        /// Exemplo: HoraPorExtenso(19, 51) --> "19 horas e 51 minutos"
        /// </summary>
        /// <param name="hora"></param>
        /// <param name="minuto"></param>
        /// <returns>Horário por extenso.</returns>
        private static string HoraPorExtenso(int hora, int minuto)
        {
            string retorno;

            switch (hora)
            {
                case 0:
                    retorno = "meia-noite";
                    break;

                case 1:
                    retorno = "1 hora";
                    break;

                case 12:
                    retorno = "meio-dia";
                    break;

                default:
                    retorno = DateTime.Now.ToString("HH") + " horas";
                    break;
            }

            switch (minuto)
            {
                case 0:
                    retorno += "";
                    break;

                case 1:
                    retorno += " e " + DateTime.Now.ToString("mm") + " minuto";
                    break;

                default:
                    retorno += " e " + DateTime.Now.ToString("mm") + " minutos";
                    break;
            }

            return retorno;
        }

        /// <summary>
        /// Retorna o nome do estado a partir de sua Unidade Federativa (UF)
        /// </summary>
        /// <param name="UF"></param>
        /// <returns>Nome do Estado por extenso.</returns>
        private static string RetornarEstado(string UF)
        {
            switch (UF)
            {
                case "AC": return "Acre";
                case "AL": return "Alagoas";
                case "AP": return "Amapá";
                case "AM": return "Amazonas";
                case "BA": return "Bahia";
                case "CE": return "Ceará";
                case "DF": return "Distrito Federal";
                case "ES": return "Espírito Santo";
                case "GO": return "Goiás";
                case "MA": return "Maranhão";
                case "MT": return "Mato Grosso";
                case "MS": return "Mato Grosso do Sul";
                case "MG": return "Minas Gerais";
                case "PA": return "Pará";
                case "PB": return "Paraíba";
                case "PR": return "Paraná";
                case "PE": return "Pernambuco";
                case "PI": return "Piauí";
                case "RJ": return "Rio de Janeiro";
                case "RN": return "Rio Grande do Norte";
                case "RS": return "Rio Grande do Sul";
                case "RO": return "Rondônia";
                case "RR": return "Roraima";
                case "SC": return "Santa Catarina";
                case "SP": return "São Paulo";
                case "SE": return "Sergipe";
                case "TO": return "Tocantins";
            }
            return "Estado Inválido!";
        }
        #endregion
    }
}
