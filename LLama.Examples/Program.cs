﻿using LLama.Examples.NewVersion;
using LLama.Native;

Console.WriteLine("======================================================================================================");

Console.WriteLine(" __       __                                       ____     __                                  \r\n/\\ \\     /\\ \\                                     /\\  _`\\  /\\ \\                                 \r\n\\ \\ \\    \\ \\ \\         __       ___ ___       __  \\ \\,\\L\\_\\\\ \\ \\___       __     _ __   _____   \r\n \\ \\ \\  __\\ \\ \\  __  /'__`\\   /' __` __`\\   /'__`\\ \\/_\\__ \\ \\ \\  _ `\\   /'__`\\  /\\`'__\\/\\ '__`\\ \r\n  \\ \\ \\L\\ \\\\ \\ \\L\\ \\/\\ \\L\\.\\_ /\\ \\/\\ \\/\\ \\ /\\ \\L\\.\\_ /\\ \\L\\ \\\\ \\ \\ \\ \\ /\\ \\L\\.\\_\\ \\ \\/ \\ \\ \\L\\ \\\r\n   \\ \\____/ \\ \\____/\\ \\__/.\\_\\\\ \\_\\ \\_\\ \\_\\\\ \\__/.\\_\\\\ `\\____\\\\ \\_\\ \\_\\\\ \\__/.\\_\\\\ \\_\\  \\ \\ ,__/\r\n    \\/___/   \\/___/  \\/__/\\/_/ \\/_/\\/_/\\/_/ \\/__/\\/_/ \\/_____/ \\/_/\\/_/ \\/__/\\/_/ \\/_/   \\ \\ \\/ \r\n                                                                                          \\ \\_\\ \r\n                                                                                           \\/_/ ");

Console.WriteLine("======================================================================================================");


NativeApi.llama_empty_call();
Console.WriteLine();

await NewVersionTestRunner.Run();