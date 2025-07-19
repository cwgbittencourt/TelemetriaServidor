<!---- Badges (substitua SEU_USUARIO e REPO) -->
![GitHub Repo stars](https://img.shields.io/github/stars/cwgbittencourt/REPO?style=social)
![GitHub last commit](https://img.shields.io/github/last-commit/cwgbittencourt/REPO)
![GitHub issues](https://img.shields.io/github/issues/cwgbittencourt/REPO)
![License](https://img.shields.io/badge/license-MIT-blue)

# TelemetriaServidor – Windows Service → SEQ

Serviço Windows em **.NET 8** que coleta métricas do sistema operacional
(CPU, memória, disco, rede, serviços, *Performance Counters*) e publica no
[**SEQ**](https://datalust.co/seq) via Serilog.

---

## 1 · Funcionalidades

| Categoria | Métricas publicadas                                  | Intervalo padrão |
|-----------|------------------------------------------------------|------------------|
| CPU       | `% Processor Time`, `% User Time`, fila de CPU       | 30 s             |
| Memória   | Uso %, MB livres, *page faults*                      | 30 s             |
| Disco     | Leituras/s, gravações/s, % Busy                      | 60 s             |
| Rede      | Bytes in/out, erros, fila NIC                        | 60 s             |
| Serviços  | Estado (`Running/Stopped`) dos serviços configurados | 30 s             |

*Os intervalos e o conjunto de métricas são **100 % configuráveis**
no `appsettings.json`.*

---

## 2 · Configuração do Serviço

```jsonc
// appsettings.json
{
  "Telemetry": {
    "IntervalSeconds": 30,
    "Metrics": [
      { "MetricName": "CPU",      "Category": "Processor", "Counter": "% Processor Time", "Instance": "_Total" },
      { "MetricName": "MemFree",  "Category": "Memory",    "Counter": "Available MBytes"  }
    ],
    "WatchedServices": [ "MSSQLSERVER", "Spooler" ]
  },
  "Serilog": {
    "MinimumLevel": "Information",
    "WriteTo": [
      {
        "Name": "Seq",
        "Args": { "serverUrl": "http://localhost:5341", "apiKey": "" }
      }
    ]
  }
}
