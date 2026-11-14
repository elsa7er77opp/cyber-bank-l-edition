namespace CyberBank.LEdition.Cli;

public static class LocalCliHelp
{
    public static string Text => """
        CYBER BANK L EDITION CLI

        A local project and evidence tool for explicitly authorized assessment records.
        It is not a scanner or attack interface.

        Safe local commands:
          project create --project <local-project-path> --name <local-label> --authorization <reference> --scope-label <declared-boundary>
          project validate --project <local-project-path>
          evidence import --project <local-project-path> --file <local-artifact-path>
          report export --project <local-project-path> --output <local-output-path>

        The CLI rejects targets, URLs, IP addresses, credentials, payloads, shell commands,
        scanner configuration, network actions, and remote storage or report delivery.
        """;
}
