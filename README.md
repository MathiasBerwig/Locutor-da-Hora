# Locutor da Hora

O Locutor da Hora é parte do projeto [Rádio, Tecnologias e Empreendedorismo na Escola](http://www.unijui.edu.br/extensao/relacao-de-projetos), uma iniciativa do programa de Extensão da Universidade Regional do Noroeste do Estado do Rio Grande do Sul ([UNIJUÍ](http://www.unijui.edu.br/)). Ele é gratuito e foi desenvolvido com o intuito de servir como ferramenta educomunicativa, auxiliando estudantes a aprimorar suas habilidades de comunicação.

![](http://locutordahora.unijui.edu.br/wp-content/uploads/2015/09/screenshot.1-min.png)

Conheça mais sobre o projeto em [nosso blog](locutordahora.unijui.edu.br).

## Funcionalidades
O aplicativo oferece ao usuário a experiência de um estúdio de rádio, passando por todas as etapas fundamentais do processo, desde a formação do texto e o preparo para a gravação até a reprodução e edição do conteúdo. Para isso, o aplicativo dispõe de interface descomplicada com botões dimensionados e dispostos de forma a facilitar o uso do aplicativo por todas as faixas etárias. Desta forma, tanto uma criança quanto um idoso podem utilizá-lo para sua formação e aprendizado. 

Na primeira interação, o usuário preenche informações básicas sobre si, que são posteriormente utilizadas para gerar textos adaptados ao seu contexto. Em seguida, é possível selecionar uma das locuções padrão ou criar locuções personalizadas. Cada locução é um conjunto de pelo menos um *título* e um *texto*, podendo ainda conter uma *trilha sonora* e um *ícone*. Após selecionar a locução desejada, o usuário pode ler o texto e iniciar a gravação. Ao avançar, é exibida a tela de edição, onde é possível realizar  alterações no material produzido, tais como adicionar uma trilha sonora, remover ou exportar trechos do áudio.

## Capturas de Tela
Confira imagens do aplicativo em [nosso blog](http://locutordahora.unijui.edu.br/o-software/).

## Licença
O Locutor da Hora é um software gratuito e de código aberto, e está licenciado sob [Creative Commons - Atribuição-NãoComercial-Compartilha Igual 4.0 Internacional](http://creativecommons.org/licenses/by-nc-sa/4.0/).

Isso significa que você pode:
* Compartilha-lo: copiar e distribuir o aplicativo em qualquer mídia ou formato
* Adapta-lo: alterar, modificar, transformar e reutilizar o aplicativo ou material disponível neste repositório

Desde que respeite:
* Atribuição: você deve dar os créditos devidos aos autores do projeto, indicar a origem do aplicativo e sua licença, além de explicitar quaisquer mudanças feitas no software.
* Utilização não comercial: você não pode utilizar esse aplicativo para fins comerciais.
* Licenciamento: se você adaptar este software, deve distribuí-lo sob a mesma licença que o original.

### Modo Exposição
Para facilitar o uso em ambientes multi-usuário, como feiras, exposições e escolas, o aplicativo dispões de funcionalidades para envio das gravações via e-mail. Para isso, é necessário que o Locutor da Hora seja iniciado com o parâmetro ``exposicao``.

Para habilitar o envio de e-mails, também é necessário que exista um arquivo chamado ``mail_template.xml`` junto ao arquivo executável. Ele deve possuir a seguinte estrutura:

```xml
<MailTemplate>
  <SmtpServer><!-- Endereço do servidor SMTP --></SmtpServer>
  <PortNumber><!-- Número da porta de comunicação --></PortNumber>
  <EnableSsl><!-- true ou false, para habilitar ou desabilitar a encriptação SSL --></EnableSsl>
  <EmailFrom><!-- Campo "De" do e-mail --></EmailFrom>
  <Username><!-- Nome de usuário do servidor SMTP  --></Username>
  <Password><!-- Senha do servidor SMTP --></Password>
  <Subject><!-- Assunto do e-mail --></Subject>
  <HtmlFile><!-- Caminho completo do arquivo HTML com o corpo da mensagem --></HtmlFile>
  <BannerImagePath><!-- Caminho completo da imagem utilizada como banner na tela de envio de e-mail --></BannerImagePath>
  <NewsletterUrl><!-- Caminho completo do formulário de assinatura do MailPoet --></NewsletterUrl>
  <NewsletterFormId><!-- Identificador do formulário --></NewsletterFormId>
  <NewsletterListId><!-- Identificador da lista que o usuário será adicionado --></NewsletterListId>
</MailTemplate>
```

## Bibliotecas Utilizadas
 * [Costura.Fody](https://github.com/Fody/Costura) para compactar e embutir dependências externas ao executável.
 * [LAME](http://lame.sourceforge.net/index.php) como encoder.
 * [NAudio](https://github.com/naudio/NAudio) como biblioteca de processamento de áudio.
 * [NAudio.Lame](https://github.com/Corey-M/NAudio.Lame) como wrapper para adicionar suporte à biblioteca LAME.
 * [WPF Sound Visualization Library](https://wpfsvl.codeplex.com/) como componentes de visualização de áudio.
 * [Wpf.Themes.ExpressionDark](https://github.com/StanislawSwierc/WpfThemesCollection) como tema dos componentes.

## Histórico de Versões
* **1.0.0 – 03/11/2015**: 
	* Lançamento oficial.

## Alterações Futuras
* Geral
	* Criar splashscreen;
	* Melhorias no padrão singleton para páginas, oferecendo menor uso de memória;
	* Conversão de formatos WAV para MP3;
	* Instalador informando os termos de uso;
	* Análise do processo de criação de locuções;
	* Alteração na estrutura dos textos da interface, permitindo desenvolvimento de versões em outros idiomas.
* Edição
	* Amplificar volume do microfone durante a edição;
	* Adicionar silêncio em uma trilha;
	* Efeitos fade-in/out;
	* Opção remover faixa;
* Configurações
	* Configurações com seleção de dispositivo de gravação;
	* Configurações com opção modo janela/tela cheia.
