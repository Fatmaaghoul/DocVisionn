FROM ollama/ollama

# Définir les variables d'environnement
ENV OLLAMA_HOST=0.0.0.0
ENV OLLAMA_ORIGINS=*

# Installer curl pour le healthcheck
RUN apt-get update && apt-get install -y curl

# Créer le script de démarrage
RUN echo '#!/bin/sh' > /start.sh && \
    echo 'ollama serve &' >> /start.sh && \
    echo 'sleep 10' >> /start.sh && \
    echo 'echo "Téléchargement de Mistral..."' >> /start.sh && \
    echo 'curl -X POST http://localhost:11434/api/pull -d "{\"name\": \"mistral\"}"' >> /start.sh && \
    echo 'curl -X POST http://localhost:11434/api/generate -d "{\"model\": \"mistral\", \"prompt\": \"\", \"options\": {\"keep_alive\": -1}}"' >> /start.sh && \
    echo 'echo "Téléchargement de LLaVA..."' >> /start.sh && \
    echo 'curl -X POST http://localhost:11434/api/pull -d "{\"name\": \"llava:7b\"}"' >> /start.sh && \
    echo 'curl -X POST http://localhost:11434/api/generate -d "{\"model\": \"llava:7b\", \"prompt\": \"\", \"options\": {\"keep_alive\": -1}}"' >> /start.sh && \
    echo 'echo "Téléchargement de Gemma3..."' >> /start.sh && \
    echo 'curl -X POST http://localhost:11434/api/pull -d "{\"name\": \"gemma3:4b\"}"' >> /start.sh && \
    echo 'curl -X POST http://localhost:11434/api/generate -d "{\"model\": \"gemma3:4b\", \"prompt\": \"\", \"options\": {\"keep_alive\": -1}}"' >> /start.sh && \
    echo 'echo "Modèles téléchargés et chargés avec succès"' >> /start.sh && \
    echo 'wait' >> /start.sh && \
    chmod +x /start.sh

# Exposer le port
EXPOSE 11434

# Healthcheck
# HEALTHCHECK --interval=30s --timeout=30s --start-period=60s --retries=3 \
#     CMD curl -f http://localhost:11434/api/version || exit 1

# Utiliser le script de démarrage
ENTRYPOINT ["/start.sh"]