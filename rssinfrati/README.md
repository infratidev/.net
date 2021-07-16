# RSSInfraTI

#### Alertas via email através do monitoramento via Zabbix  convertendo os emails para rss e exibindo em uma página.
Em um ambiente on-premise, com várias filiais, cada qual com sua equipe de TI, compartilhando recursos, links oticos, etc.,
surgiu a idéia de criar uma pagina, que ficasse disponivel para todas as equipes de TI das filiais, exibindo alertas dos mais variados tipos, por ex: indisponibilidade de links, servidores, manutenção em banco de dados, etc. Uma forma a mais, para as filias acompanharem as ocorrencias.

As filiais todas, utilizam solução do Gsuite.
A ideia inicial era o zabbix disparar o alerta tanto via webhook nas salas do Gchat (cada filial tem a sua propria sala), quando via email em uma conta especifica, e nessa conta o inbox das mensagens seria lido, convertido para rss, e atraves de uma pagina aps.net, exibir o rss feed.

Inicialmente a conversão do inbox para rss feed, utilizou-se o serviço do [Volodya Shtenovych](https://emails2rss.appspot.com/), excelente ferramenta, porem existia um limite de notificações que deixava em determinados periodos do dia, offline o serviço.

Partimos para outra solução, com a mesma finalidade, porém sem custo.

Atráves do Gmail Inbox feed - Atom, consumir o XML e converter para o padrão desejado utilizando OAuth2.0 com refresh token e exibir o feed. O controle do tempo de expiração da mensagem via google app script na propria conta do email.

Como todo o ambiente interno entre filiais é local, independe de disponibilidade de internet, foi desenvolvido um outro modulo em php, para criar manualmente o rss feed, e disponibilizar para essa mesma aplicação ler, caso houver indisponibilidade de acesso ao gmail. Mas o sistema irá ler ambos, local e rss feed atom. Se ambos tiverem alertas, todos serão exibidos.








