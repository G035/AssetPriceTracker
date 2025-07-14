# Asset Price Tracker

## Introdução
Essa aplicação monitora preços de ativos e notifica por email nos momentos adequados para compra e venda desses ativos.

## Configuração
Os endereços de email dos destinatários devem ser definidos no arquivo de configuração `appsettings.json` junto das configurações do servidor SMTP para envio de emails e o período de verificação do preço dos ativos. É fornecido um arquivo `appsettings.json.example` como inspiração.

## Compilação
Os binários podem ser construídos com `make`. Para contruir os binários para Windows e Linux, execute `make publish-all`.

## Execução
Após a compilação, os binários estarão em `publish/windows/AssetPriceTracker` e `publish/linux/AssetPriceTracker`.

**ATENÇÃO:** Os preços de referência devem ser definidos na moeda adequada. Por exemplo: USD para AAPL, BRL para PETR4, CHF para UBSG etc.

Para monitorar o preço do ticket "AAPL", compreço de venda 500.00 e preço de compra 20.00 por exemplo: `AssetPriceTracker AAPL 500.00 20.00`.

Se tiver qualquer dúvida, pode obter mais informações com `AssetPriceTracker -h`