import logging
from deep_translator import GoogleTranslator
import re

def capitalize_after_period(text):
    # Lowercase entire text first
    text = text.lower()

    # Capitalize first character after a period (and optional spaces)
    def repl(match):
        return match.group(1) + match.group(2).upper()
    return re.sub(r'(\.|\A)(\s*)([a-z])', lambda m: m.group(1) + m.group(2) + m.group(3).upper(), text)


# Configurer le logging
logging.basicConfig(level=logging.INFO, format='%(asctime)s - %(levelname)s - %(message)s')
logger = logging.getLogger(__name__)

def translate_to_french(text: str) -> str:
    """
    Traduit un texte en français en utilisant Google Translator, en gérant les textes longs.

    Args:
        text (str): Le texte à traduire.

    Returns:
        str: Le texte traduit en français.

    Raises:
        ValueError: Si le texte est vide ou contient uniquement des espaces.
        Exception: Si une erreur survient lors de la traduction.
    """
    if not text.strip():
        logger.error("Le texte est vide ou contient uniquement des espaces")
        raise ValueError("Le texte ne peut pas être vide")

    try:
        logger.info(f"Longueur du texte d'entrée : {len(text)} caractères")
        # Limite de caractères par segment (Google Translator)
        max_length = 5000
        # Diviser le texte en segments de moins de 5000 caractères
        segments = []
        current_segment = ""
        for line in text.splitlines(keepends=True):
            if len(current_segment) + len(line) <= max_length:
                current_segment += line
            else:
                segments.append(current_segment)
                current_segment = line
        if current_segment:
            segments.append(current_segment)

        logger.info(f"Nombre de segments créés : {len(segments)}")

        # Traduire chaque segment
        translated_segments = []
        for i, segment in enumerate(segments):
            logger.info(f"Traduction du segment {i+1}/{len(segments)} (longueur : {len(segment)} caractères)")
            translator = GoogleTranslator(source='en', target='fr')  # Forcer la langue source à l'anglais
            translated_segment = translator.translate(segment)
            if translated_segment is None:
                logger.warning(f"Le segment {i+1} n'a pas été traduit (retourné None)")
                translated_segment = segment  # Conserver le segment original si la traduction échoue
            translated_segments.append(translated_segment)
            logger.info(f"Segment {i+1} traduit avec succès")

        # Recombiner les segments traduits
        translated_text = "".join(translated_segments)
        logger.info(f"Longueur du texte traduit : {len(translated_text)} caractères")
        translated_text = capitalize_after_period(translated_text)
        return translated_text
    except Exception as e:
        logger.error(f"Erreur lors de la traduction : {str(e)}")
        raise Exception(f"Erreur lors de la traduction : {str(e)}")