<template>
  <div class="home-container" >
    <!-- Hero Section -->
    <section class="hero-section">
      <div class="hero-overlay"></div>
      <div class="hero-content">
        <div class="hero-badge">Propulsé par l'IA</div>
        <h1>Révolutionnez votre <span class="highlight">gestion documentaire</span></h1>
        <p class="hero-description">
          Transformez vos documents en insights exploitables grâce à notre plateforme intelligente.
          Extraction automatique, analyse avancée et organisation intuitive en quelques clics.
        </p>
        <div class="hero-buttons">
          <button class="primary-btn" @click="handleGetStarted">Démarrer gratuitement</button>
          <button class="secondary-btn" @click="scrollToFeatures">Découvrir les fonctionnalités</button>
        </div>
        <div class="hero-stats">
          <div class="hero-stat">
            <span class="stat-value">98%</span>
            <span class="stat-label">Précision d'extraction</span>
          </div>
          <div class="hero-stat">
            <span class="stat-value">+60%</span>
            <span class="stat-label">Gain de productivité</span>
          </div>
          <div class="hero-stat">
            <span class="stat-value">24/7</span>
            <span class="stat-label">Support client</span>
          </div>
        </div>
      </div>
      <div class="hero-image">
        <div class="document-preview">
          <div class="document-header">
            <div class="doc-icon"><i class="bi bi-file-earmark-text"></i></div>
            <div class="doc-title">Rapport_Analyse.pdf</div>
          </div>
          <div class="document-body">
            <div class="doc-text-line"></div>
            <div class="doc-text-line"></div>
            <div class="doc-text-line short"></div>
            <div class="doc-image"></div>
            <div class="doc-text-line"></div>
            <div class="doc-text-line short"></div>
          </div>
          <div class="extraction-beam"></div>
        </div>
      </div>
    </section>

    <!-- Features Section -->
    <section class="features-section">
      <div class="section-header">
        <h2>Fonctionnalités principales</h2>
        <p>Découvrez comment notre plateforme peut transformer votre gestion documentaire</p>
      </div>

      <div class="features-grid">
        <div class="feature-card">
          <div class="feature-icon">
            <i class="bi bi-file-earmark-text"></i>
          </div>
          <h3>Extraction intelligente</h3>
          <p>Notre IA extrait automatiquement le contenu et les images de vos documents pour une analyse approfondie.</p>
        </div>

        <div class="feature-card">
          <div class="feature-icon">
            <i class="bi bi-search"></i>
          </div>
          <h3>Recherche avancée</h3>
          <p>Trouvez rapidement les informations dont vous avez besoin grâce à notre moteur de recherche puissant.</p>
        </div>

        <div class="feature-card">
          <div class="feature-icon">
            <i class="bi bi-shield-check"></i>
          </div>
          <h3>Sécurité renforcée</h3>
          <p>Vos documents sont protégés par des mesures de sécurité de pointe pour garantir la confidentialité.</p>
        </div>
        
        <div class="feature-card">
          <div class="feature-icon">
            <i class="bi bi-image"></i>
          </div>
          <h3>Analyse contextuelle d'images</h3>
          <p>Notre IA analyse les images en tenant compte du contexte du document pour en extraire des insights pertinents.</p>
        </div>
        
        <div class="feature-card">
          <div class="feature-icon">
            <i class="bi bi-journal-text"></i>
          </div>
          <h3>Résumé </h3>
          <p>Générez des résumés concis et pertinents de vos documents pour gagner du temps et faciliter la compréhension.</p>
        </div>
        
        <div class="feature-card">
          <div class="feature-icon">
            <i class="bi bi-chat-square-text"></i>
          </div>
          <h3>Description d'objets visuels</h3>
          <p>Obtenez des descriptions détaillées des objets identifiés dans vos images pour une meilleure compréhension.</p>
        </div>
      </div>
    </section>

    <!-- How It Works Section -->
    <section class="how-it-works-section">
      <div class="section-header">
        <h2>Comment ça fonctionne</h2>
        <p>Un processus simple en trois étapes pour gérer vos documents efficacement</p>
      </div>

      <div class="steps-container">
        <div class="step-card">
          <div class="step-number">1</div>
          <h3>Téléchargez</h3>
          <p>Importez vos documents PDF dans notre plateforme sécurisée.</p>
        </div>

        <div class="step-card">
          <div class="step-number">2</div>
          <h3>Analysez</h3>
          <p>Notre IA extrait automatiquement le contenu et les images.</p>
        </div>

        <div class="step-card">
          <div class="step-number">3</div>
          <h3>Gérez</h3>
          <p>Accédez, recherchez et partagez vos documents facilement.</p>
        </div>
      </div>
    </section>

    <!-- Stats Section -->
    <section class="stats-section">
      <div class="stats-container">
        <div class="stat-card">
          <div class="stat-number">1000+</div>
          <div class="stat-label">Documents traités</div>
        </div>

        <div class="stat-card">
          <div class="stat-number">98%</div>
          <div class="stat-label">Précision d'extraction</div>
        </div>

        <div class="stat-card">
          <div class="stat-number">24/7</div>
          <div class="stat-label">Disponibilité</div>
        </div>
      </div>
    </section>

    <!-- CTA Section -->
    <section class="cta-section">
      <div class="cta-content">
        <h2>Prêt à simplifier votre gestion documentaire ?</h2>
        <p>Rejoignez notre plateforme dès aujourd'hui et découvrez la puissance de l'IA pour vos documents.</p>
        <button class="cta-btn" @click="handleGetStarted">Commencer maintenant</button>
      </div>
    </section>
  <Footer></Footer>
  </div>
</template>

<script>
import Footer from '@/components/Footer.vue';

export default {
  name: 'Home',
  components: {
  Footer
},
  methods: {
    handleGetStarted() {
      // Accède à l'état authentification depuis Vuex
      const authenticated = this.$store.state.authenticated;
      console.log('Bouton cliqué');
      console.log('Utilisateur authentifié:', authenticated); // Vérification de l'authentification

      if (authenticated) {
        this.$router.push('/document'); // Si authentifié, rediriger vers /document
      } else {
        this.$router.push('/login'); // Sinon, rediriger vers /login
      }
    },
    scrollToFeatures() {
      // Faire défiler jusqu'à la section des fonctionnalités
      const featuresSection = document.querySelector('.features-section');
      if (featuresSection) {
        featuresSection.scrollIntoView({ behavior: 'smooth' });
      }
    }
  }
}
</script>

<style scoped>
.home-container {
  margin-top: 3%;
  width: 100%;
  overflow-x: hidden;
}

/* Hero Section */
.hero-section {
  position: relative;
  background: linear-gradient(135deg, #4F46E5 0%, #7C3AED 100%);
  color: white;
  padding: 100px 20px;
  display: flex;
  overflow: hidden;
  min-height: 600px;
}

.hero-overlay {
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background-image: url("data:image/svg+xml,%3Csvg width='60' height='60' viewBox='0 0 60 60' xmlns='http://www.w3.org/2000/svg'%3E%3Cg fill='none' fill-rule='evenodd'%3E%3Cg fill='%23ffffff' fill-opacity='0.05'%3E%3Cpath d='M36 34v-4h-2v4h-4v2h4v4h2v-4h4v-2h-4zm0-30V0h-2v4h-4v2h4v4h2V6h4V4h-4zM6 34v-4H4v4H0v2h4v4h2v-4h4v-2H6zM6 4V0H4v4H0v2h4v4h2V6h4V4H6z'/%3E%3C/g%3E%3C/g%3E%3C/svg%3E");
  opacity: 0.8;
}

.hero-content {
  flex: 1;
  max-width: 600px;
  margin-right: 100px;
  margin-left :100px ;
  position: relative;
  z-index: 1;
}

.hero-badge {
  display: inline-block;
  background-color: rgba(255, 255, 255, 0.15);
  color: white;
  padding: 8px 16px;
  border-radius: 20px;
  font-size: 0.9rem;
  font-weight: 500;
  margin-bottom: 20px;
  backdrop-filter: blur(5px);
  border: 1px solid rgba(255, 255, 255, 0.2);
}

.hero-section h1 {
  font-size: 3.5rem;
  font-weight: 800;
  margin-bottom: 25px;
  line-height: 1.2;
  letter-spacing: -0.5px;
}

.highlight {
  position: relative;
  display: inline-block;
  z-index: 1;
}

.highlight::after {
  content: '';
  position: absolute;
  bottom: 5px;
  left: 0;
  width: 100%;
  height: 12px;
  background-color: rgba(255, 255, 255, 0.2);
  z-index: -1;
  border-radius: 10px;
}

.hero-description {
  font-size: 1.25rem;
  margin-bottom: 40px;
  line-height: 1.7;
  opacity: 0.9;
}

.hero-buttons {
  display: flex;
  gap: 15px;
  margin-bottom: 50px;
  flex-wrap: wrap;
}

.primary-btn {
  background-color: white;
  color: #4F46E5;
  padding: 15px 30px;
  border-radius: 8px;
  border: none;
  font-size: 1.125rem;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.3s ease;
  box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.1);
}

.primary-btn:hover {
  transform: translateY(-2px);
  box-shadow: 0 6px 15px -3px rgba(0, 0, 0, 0.2);
}

.secondary-btn {
  background-color: rgba(255, 255, 255, 0.1);
  color: white;
  padding: 15px 30px;
  border-radius: 8px;
  border: 1px solid rgba(255, 255, 255, 0.2);
  font-size: 1.125rem;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.3s ease;
  backdrop-filter: blur(5px);
}

.secondary-btn:hover {
  background-color: rgba(255, 255, 255, 0.2);
}

.hero-stats {
  display: flex;
  gap: 30px;
  flex-wrap: wrap;
}

.hero-stat {
  display: flex;
  flex-direction: column;
}

.stat-value {
  font-size: 2rem;
  font-weight: 700;
  margin-bottom: 5px;
}

.stat-label {
  font-size: 0.9rem;
  opacity: 0.8;
}

.hero-image {
  flex: 1;
  display: flex;
  align-items: center;
  justify-content: center;
  position: relative;
  z-index: 1;
}

.document-preview {
  width: 350px;
  height: 450px;
  background-color: white;
  border-radius: 12px;
  box-shadow: 0 20px 40px rgba(0, 0, 0, 0.2);
  overflow: hidden;
  position: relative;
  transform: perspective(1000px) rotateY(-10deg) rotateX(5deg);
}

.document-header {
  display: flex;
  align-items: center;
  padding: 15px;
  background-color: #f8f9fa;
  border-bottom: 1px solid #e9ecef;
}

.doc-icon {
  width: 30px;
  height: 30px;
  background-color: #4F46E5;
  color: white;
  border-radius: 6px;
  display: flex;
  align-items: center;
  justify-content: center;
  margin-right: 10px;
}

.doc-title {
  font-weight: 600;
  color: #333;
}

.document-body {
  padding: 20px;
}

.doc-text-line {
  height: 12px;
  background-color: #e9ecef;
  border-radius: 6px;
  margin-bottom: 15px;
  width: 100%;
}

.doc-text-line.short {
  width: 70%;
}

.doc-image {
  height: 150px;
  background-color: #e9ecef;
  border-radius: 6px;
  margin-bottom: 15px;
  background-image: linear-gradient(45deg, #f1f3f5 25%, transparent 25%, transparent 50%, #f1f3f5 50%, #f1f3f5 75%, transparent 75%, transparent);
  background-size: 20px 20px;
}

.extraction-beam {
  position: absolute;
  top: 0;
  left: 0;
  width: 100%;
  height: 3px;
  background-color: #4F46E5;
  box-shadow: 0 0 10px 2px rgba(79, 70, 229, 0.5);
  animation: scan 3s infinite;
}

@keyframes scan {
  0% {
    top: 0;
  }
  50% {
    top: 100%;
  }
  100% {
    top: 0;
  }
}

/* Section Header */
.section-header {
  text-align: center;
  margin-bottom: 60px;
}

.section-header h2 {
  font-size: 2.5rem;
  color: #1F2937;
  margin-bottom: 15px;
}

.section-header p {
  font-size: 1.125rem;
  color: #6B7280;
  max-width: 600px;
  margin: 0 auto;
}

/* Features Section */
.features-section {
  padding: 100px 20px;
  background-color: #F9FAFB;
}

.features-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(300px, 1fr));
  gap: 30px;
  max-width: 1200px;
  margin: 0 auto;
}

.feature-card {
  background-color: white;
  border-radius: 12px;
  padding: 30px;
  box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.1);
  transition: all 0.3s ease;
  text-align: center;
}

.feature-card:hover {
  transform: translateY(-5px);
  box-shadow: 0 10px 25px -5px rgba(0, 0, 0, 0.1);
}

.feature-icon {
  font-size: 2.5rem;
  color: #4F46E5;
  margin-bottom: 20px;
}

.feature-card h3 {
  font-size: 1.5rem;
  color: #1F2937;
  margin-bottom: 15px;
}

.feature-card p {
  color: #6B7280;
  line-height: 1.6;
}

/* How It Works Section */
.how-it-works-section {
  padding: 100px 20px;
  background-color: white;
}

.steps-container {
  display: flex;
  flex-wrap: wrap;
  justify-content: center;
  gap: 30px;
  max-width: 1200px;
  margin: 0 auto;
}

.step-card {
  flex: 1;
  min-width: 250px;
  max-width: 350px;
  background-color: #F9FAFB;
  border-radius: 12px;
  padding: 30px;
  text-align: center;
  position: relative;
}

.step-number {
  position: absolute;
  top: -20px;
  left: 50%;
  transform: translateX(-50%);
  width: 40px;
  height: 40px;
  background-color: #4F46E5;
  color: white;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  font-weight: 700;
  font-size: 1.25rem;
}

.step-card h3 {
  font-size: 1.5rem;
  color: #1F2937;
  margin: 20px 0 15px;
}

.step-card p {
  color: #6B7280;
  line-height: 1.6;
}

/* Stats Section */
.stats-section {
  padding: 80px 20px;
  background-color: #4F46E5;
  color: white;
}

.stats-container {
  display: flex;
  flex-wrap: wrap;
  justify-content: center;
  gap: 40px;
  max-width: 1200px;
  margin: 0 auto;
}

.stat-card {
  text-align: center;
  flex: 1;
  min-width: 200px;
}

.stat-number {
  font-size: 3rem;
  font-weight: 700;
  margin-bottom: 10px;
}

.stat-label {
  font-size: 1.125rem;
  opacity: 0.9;
}

/* CTA Section */
.cta-section {
  padding: 100px 20px;
  background-color: #F9FAFB;
  text-align: center;
}

.cta-content {
  max-width: 800px;
  margin: 0 auto;
}

.cta-section h2 {
  font-size: 2.5rem;
  color: #1F2937;
  margin-bottom: 20px;
}

.cta-section p {
  font-size: 1.125rem;
  color: #6B7280;
  margin-bottom: 40px;
}

.cta-btn {
  background-color: #4F46E5;
  color: white;
  padding: 15px 40px;
  border-radius: 9999px;
  border: none;
  font-size: 1.125rem;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.3s ease;
  box-shadow: 0 4px 6px -1px rgba(79, 70, 229, 0.3);
}

.cta-btn:hover {
  background-color: #4338CA;
  transform: translateY(-2px);
  box-shadow: 0 6px 15px -3px rgba(79, 70, 229, 0.4);
}

/* Responsive Design */
@media (max-width: 1024px) {
  .hero-section {
    flex-direction: column;
    padding: 60px 20px;
  }

  .hero-content {
    max-width: 100%;
    margin-right: 0;
    margin-bottom: 60px;
    text-align: center;
  }

  .hero-buttons {
    justify-content: center;
  }

  .hero-stats {
    justify-content: center;
  }

  .document-preview {
    transform: perspective(1000px) rotateY(0) rotateX(0);
  }
}

@media (max-width: 768px) {
  .hero-section {
    padding: 40px 20px;
  }

  .hero-section h1 {
    font-size: 2.5rem;
  }

  .hero-description {
    font-size: 1.125rem;
  }

  .hero-buttons {
    flex-direction: column;
    width: 100%;
    max-width: 300px;
    margin-left: auto;
    margin-right: auto;
  }

  .document-preview {
    width: 280px;
    height: 380px;
  }

  .section-header h2 {
    font-size: 2rem;
  }

  .features-grid {
    grid-template-columns: 1fr;
  }

  .steps-container {
    flex-direction: column;
    align-items: center;
  }

  .step-card {
    width: 100%;
  }

  .stats-container {
    flex-direction: column;
    gap: 30px;
  }

  .cta-section h2 {
    font-size: 2rem;
  }
}

@media (max-width: 480px) {
  .hero-section h1 {
    font-size: 2rem;
  }

  .hero-badge {
    font-size: 0.8rem;
  }

  .hero-stats {
    flex-direction: column;
    align-items: center;
    gap: 20px;
  }

  .document-preview {
    width: 240px;
    height: 320px;
  }
}
</style>