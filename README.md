# SubClick

[English](docs/README.en.md)

Busque legendas pelo botão direito do Windows. O SubClick abre uma janela pequena, pesquisa automaticamente por hash e recorre ao título quando necessário. Você escolhe a versão e a legenda é salva junto ao vídeo.

## Uso

1. Instale com `SubClick-0.1.0-win-x64-setup.exe` e mantenha marcada a integração ao menu de contexto.
2. Clique com o botão direito em um vídeo → **Mostrar mais opções** → **Buscar legendas — SubClick**.
3. Na primeira execução, escolha seu idioma padrão. A sugestão inicial é português brasileiro.
4. Selecione um resultado e clique em **Baixar selecionada**, ou dê um duplo clique.

Você também pode abrir o SubClick pelo menu Iniciar e escolher o vídeo. É possível ajustar o título e trocar o idioma de uma busca. **Usar como padrão** salva o idioma para as próximas utilizações. A interface pode seguir o Windows ou usar português/inglês.

Os arquivos são salvos como `Video.pob.srt`, `Video.eng.srt`, `Video.jpn.srt` etc. O código é o identificador do serviço; `pob` significa português brasileiro e `por`, português de Portugal. Legendas existentes nunca são sobrescritas.

## Requisitos e limites

- Windows 11 x64 e conexão à internet. VLC e .NET não precisam estar instalados.
- Um vídeo por vez; legendas SRT completas, de uma parte. Legendas somente de trechos estrangeiros são excluídas; SDH é indicada nos resultados.
- Todos os idiomas oferecidos pelo serviço; catálogo inicial de 93 idiomas e atualização pela rede, com cópia local.
- Acesso anônimo pelo protocolo legado utilizado pelo VLSub. Não pede conta nem chave pessoal, mas o serviço pode alterar disponibilidade e limites. O SubClick não contorna essas restrições.
- O instalador inicial não possui assinatura digital.

## Preferências e privacidade

Preferências em `%LOCALAPPDATA%\SubClick\settings.json`, catálogo em `languages.json`. Atualizações e desinstalação preservam essa pasta. Para restaurar os padrões, feche o programa e remova manualmente `settings.json`.

O vídeo não é enviado. A busca transmite ao OpenSubtitles o idioma e o hash/tamanho ou título do vídeo. Não há telemetria, publicidade nem serviço em segundo plano.

## Desenvolvimento e distribuição

Veja [como compilar e testar](docs/DEVELOPMENT.md) e [a lista de validação de versões](docs/RELEASING.md). Instaladores serão disponibilizados nas [Releases](https://github.com/KevinColeti/SubClick/releases).

GPL-3.0-or-later. Atribuições e origem do código em [THIRD-PARTY-NOTICES.md](THIRD-PARTY-NOTICES.md). Projeto independente, sem afiliação oficial ao VLC ou OpenSubtitles.
