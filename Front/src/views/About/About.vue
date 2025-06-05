<template>
  <div class="about-container">
    <div class="about-header">
      <div class="header-content">
        <h1>Guide d'utilisation</h1>
        <p class="subtitle">Découvrez comment tirer le meilleur parti de notre plateforme</p>
      </div>
    </div>

    <div class="about-content">
      <div class="welcome-section">
        <div class="welcome-icon">
          <i class="bi bi-file-earmark-text-fill"></i>
        </div>
        <h2>Bienvenue sur votre gestionnaire de documents intelligent</h2>
        <p>Notre application vous permet de gérer, analyser et extraire des informations de vos documents PDF grâce à notre technologie d'intelligence artificielle avancée.</p>
      </div>

      <div class="steps-container">
        <div class="step-card">
          <div class="step-number">1</div>
          <div class="step-content">
            <h3>Ajouter des documents</h3>
            <p>Depuis la page "Mes Documents", cliquez sur la carte "Ajouter un nouveau document" ou sur le bouton d'ajout. Vous pouvez glisser-déposer vos fichiers PDF ou les sélectionner depuis votre ordinateur.</p>
            <div class="step-icon">
              <i class="bi bi-cloud-upload"></i>
            </div>
            <div class="step-tips">
              <h4>Conseils :</h4>
              <ul>
                <li>Vous pouvez téléverser plusieurs documents à la fois</li>
                <li>Donnez des noms clairs à vos documents pour les retrouver facilement</li>
                <li>Ajoutez une description détaillée pour faciliter les recherches futures</li>
              </ul>
            </div>
          </div>
        </div>

        <div class="step-card">
          <div class="step-number">2</div>
          <div class="step-content">
            <h3>Consulter et rechercher vos documents</h3>
            <p>Tous vos documents sont affichés sous forme de cartes dans la section "Mes Documents". Utilisez la barre de recherche pour trouver rapidement un document par son nom, sa description ou son contenu.</p>
            <div class="step-icon">
              <i class="bi bi-search"></i>
            </div>
            <div class="step-tips">
              <h4>Fonctionnalités de recherche :</h4>
              <ul>
                <li>Filtrez par date d'ajout pour retrouver vos documents récents</li>
                <li>La recherche s'effectue dans le nom, la description et le contenu textuel</li>
                <li>Les documents sont triés par date, les plus récents apparaissant en premier</li>
              </ul>
            </div>
          </div>
        </div>

        <div class="step-card">
          <div class="step-number">3</div>
          <div class="step-content">
            <h3>Analyser le contenu de vos documents</h3>
            <p>Cliquez sur un document pour accéder à son contenu détaillé. Notre système analyse automatiquement votre document et extrait les informations importantes.</p>
            <div class="step-icon">
              <i class="bi bi-graph-up"></i>
            </div>
            <div class="step-features">
              <div class="feature-pill">
                <i class="bi bi-image"></i>
                <span>Extraction d'images</span>
              </div>
              <div class="feature-pill">
                <i class="bi bi-file-text"></i>
                <span>Extraction de texte</span>
              </div>
              <div class="feature-pill">
                <i class="bi bi-journal-text"></i>
                <span>Génération de résumés</span>
              </div>
              <div class="feature-pill">
                <i class="bi bi-image"></i>
                <span>Génération des descriptions</span>
              </div>
            </div>
          </div>
        </div>

        <div class="step-card">
          <div class="step-number">4</div>
          <div class="step-content">
            <h3>Gérer vos documents</h3>
            <p>Vous pouvez facilement télécharger, modifier ou supprimer vos documents à tout moment. Sélectionnez plusieurs documents pour effectuer des actions groupées.</p>
            <div class="step-icon">
              <i class="bi bi-gear"></i>
            </div>
            <div class="step-actions">
              <div class="action-button">
                <i class="bi bi-download"></i>
                <span>Télécharger</span>
              </div>
              <div class="action-button">
                <i class="bi bi-pencil"></i>
                <span>Modifier</span>
              </div>
              <div class="action-button">
                <i class="bi bi-trash"></i>
                <span>Supprimer</span>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- <div class="help-section">
        <h2>Besoin d'aide supplémentaire ?</h2>
        <p>Si vous avez des questions ou rencontrez des problèmes, notre équipe de support est là pour vous aider.</p>
        <div class="help-options">
          <a href="#" class="help-card">
            <i class="bi bi-question-circle"></i>
            <h3>FAQ</h3>
            <p>Consultez notre base de connaissances pour trouver des réponses à vos questions.</p>
          </a>
          <a href="#" class="help-card">
            <i class="bi bi-chat-dots"></i>
            <h3>Support</h3>
            <p>Contactez notre équipe de support pour une assistance personnalisée.</p>
          </a>
          <a href="#" class="help-card">
            <i class="bi bi-book"></i>
            <h3>Tutoriels</h3>
            <p>Découvrez des guides détaillés pour maîtriser toutes les fonctionnalités.</p>
          </a>
        </div>
      </div> -->
    </div>
 
  </div>
  <Footer></Footer>
</template>

<script>
import Footer from '@/components/Footer.vue';

export default {
  name: 'About',
  components: {
  Footer
}

}


</script>

<style scoped>
.about-container {
  max-width: 100%;
  margin: 80px auto;
  padding: 0 20px;
}

.about-header {
  background: linear-gradient(135deg, #4F46E5 0%, #2196F3 100%);
  border-radius: 12px;
  padding: 180px 40px;
  margin-bottom: 40px;
  text-align: center;
  color: white;
  box-shadow: 0 10px 30px rgba(0, 0, 0, 0.1);
}

.header-content h1 {
  font-size: 2.8rem;
  margin-bottom: 15px;
  font-weight: 700;
}

.header-content .subtitle {
  font-size: 1.2rem;
  opacity: 0.9;
  max-width: 600px;
  margin: 0 auto;
}

.about-content {
  padding: 20px 0;
}

.welcome-section {
  text-align: center;
  margin-bottom: 60px;
  padding: 30px;
  background-color: white;
  border-radius: 12px;
  box-shadow: 0 5px 15px rgba(0, 0, 0, 0.05);
}

.welcome-icon {
  width: 80px;
  height: 80px;
  background-color: #4F46E5;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  margin: 0 auto 20px;
}

.welcome-icon i {
  font-size: 2.5rem;
  color: white;
}

.welcome-section h2 {
  font-size: 2rem;
  color: #333;
  margin-bottom: 15px;
}

.welcome-section p {
  font-size: 1.1rem;
  color: #666;
  max-width: 800px;
  margin: 0 auto;
  line-height: 1.6;
}

.steps-container {
  display: flex;
  flex-direction: column;
  gap: 30px;
  margin-bottom: 60px;
}

.step-card {
  display: flex;
  background-color: white;
  border-radius: 12px;
  overflow: hidden;
  box-shadow: 0 5px 15px rgba(0, 0, 0, 0.05);
}

.step-number {
  display: flex;
  align-items: center;
  justify-content: center;
  min-width: 80px;
  font-size: 2.5rem;
  font-weight: 700;
  color: white;
  background-color: #4F46E5;
}

.step-content {
  flex: 1;
  padding: 30px;
  position: relative;
}

.step-content h3 {
  font-size: 1.5rem;
  color: #333;
  margin-bottom: 15px;
  padding-right: 60px;
}

.step-content p {
  color: #666;
  line-height: 1.6;
  margin-bottom: 20px;
}

.step-icon {
  position: absolute;
  top: 25px;
  right: 30px;
  width: 50px;
  height: 50px;
  background-color: #f0f7ff;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
}

.step-icon i {
  font-size: 1.5rem;
  color: #4F46E5;
}

.step-tips, .step-features {
  background-color: #f8f9fa;
  border-radius: 8px;
  padding: 20px;
  margin-top: 20px;
}

.step-tips h4 {
  font-size: 1.1rem;
  color: #333;
  margin-bottom: 10px;
}

.step-tips ul {
  padding-left: 20px;
}

.step-tips li {
  margin-bottom: 8px;
  color: #555;
}

.step-features {
  display: flex;
  flex-wrap: wrap;
  gap: 10px;
}

.feature-pill {
  display: flex;
  align-items: center;
  gap: 8px;
  background-color: white;
  border-radius: 30px;
  padding: 8px 15px;
  box-shadow: 0 2px 5px rgba(0, 0, 0, 0.05);
}

.feature-pill i {
  color: #4F46E5;
}

.step-actions {
  display: flex;
  gap: 15px;
  margin-top: 20px;
}

.action-button {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 5px;
  padding: 15px;
  border-radius: 8px;
  background-color: #f8f9fa;
  transition: all 0.3s ease;
  cursor: pointer;
}

.action-button:hover {
  background-color: #e9ecef;
  transform: translateY(-3px);
}

.action-button i {
  font-size: 1.5rem;
  color: #4F46E5;
}

.action-button span {
  font-size: 0.9rem;
  color: #555;
}

.help-section {
  text-align: center;
  padding: 40px;
  background-color: white;
  border-radius: 12px;
  box-shadow: 0 5px 15px rgba(0, 0, 0, 0.05);
}

.help-section h2 {
  font-size: 2rem;
  color: #333;
  margin-bottom: 15px;
}

.help-section > p {
  font-size: 1.1rem;
  color: #666;
  max-width: 700px;
  margin: 0 auto 30px;
}

.help-options {
  display: flex;
  justify-content: center;
  gap: 30px;
  flex-wrap: wrap;
}

.help-card {
  flex: 1;
  min-width: 250px;
  max-width: 300px;
  padding: 30px 20px;
  background-color: #f8f9fa;
  border-radius: 12px;
  text-align: center;
  transition: all 0.3s ease;
  text-decoration: none;
}

.help-card:hover {
  transform: translateY(-5px);
  box-shadow: 0 10px 20px rgba(0, 0, 0, 0.1);
}

.help-card i {
  font-size: 2.5rem;
  color: #4F46E5;
  margin-bottom: 15px;
}

.help-card h3 {
  font-size: 1.3rem;
  color: #333;
  margin-bottom: 10px;
}

.help-card p {
  font-size: 0.95rem;
  color: #666;
}

@media (max-width: 768px) {
  .about-header {
    padding: 40px 20px;
  }

  .header-content h1 {
    font-size: 2.2rem;
  }

  .step-card {
    flex-direction: column;
  }

  .step-number {
    min-width: 100%;
    padding: 15px 0;
  }

  .step-content {
    padding: 20px;
  }

  .step-icon {
    position: static;
    margin: 0 0 15px auto;
  }

  .step-content h3 {
    padding-right: 0;
  }

  .help-card {
    min-width: 100%;
  }
}
</style>
