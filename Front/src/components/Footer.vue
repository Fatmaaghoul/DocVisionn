<template>
  <footer class="footer">
    <div class="footer-content container">
      <!-- Section newsletter -->
      <div class="newsletter-section">
        <h3>Rejoignez-nous</h3>
        <p>Connectez-vous pour accéder à toutes les fonctionnalités de DocuVision.</p>
        <div class="connect-button-container">
          <router-link to="/login" class="connect-button">
            <span>Se connecter</span>
            <i class="bi bi-arrow-right"></i>
          </router-link>
        </div>
        <div class="social-icons">
          <a href="#" aria-label="Facebook"><i class="bi bi-facebook"></i></a>
          <a href="#" aria-label="Instagram"><i class="bi bi-instagram"></i></a>
          <a href="#" aria-label="Twitter"><i class="bi bi-twitter-x"></i></a>
          <a href="#" aria-label="LinkedIn"><i class="bi bi-linkedin"></i></a>
        </div>
      </div>

      <!-- Section liens -->
      <div class="footer-links">
        <div class="link-column">
          <h4>Contactez-nous</h4>
          <ul>
            <li><a href="#">Nous joindre</a></li>
            <li><a href="#">À propos</a></li>
            <li><a href="#">FAQ</a></li>
            <li><a href="#">Confidentialité</a></li>
          </ul>
        </div>

        <div class="link-column">
          <h4>DocuVision</h4>
          <ul>
            <li><router-link to="/About">Conditions d'utilisation</router-link></li>
            <li><router-link to="/About">Livraison</router-link></li>
            <li><router-link to="/About">Politique de confidentialité</router-link></li>
          </ul>
        </div>

        <div class="link-column">
          <h4>Ressources</h4>
          <ul>
            <li><router-link to="/About">Blog</router-link></li>
            <li><router-link to="/About">Tutoriels</router-link></li>
            <li><router-link to="/About">API</router-link></li>
            <li><router-link to="/About">Support</router-link></li>
          </ul>
        </div>
      </div>
    </div>

    <!-- Barre de copyright -->
    <div class="footer-bottom">
      <div class="container footer-bottom-content">
        <div class="logo-copyright">
          <img src="@/assets/logo.png" alt="DocuVision" class="footer-logo" />
          <p>© {{ currentYear }} DocuVision. Tous droits réservés.</p>
        </div>
      
      </div>
    </div>
  </footer>
</template>

<script setup>
import { computed } from 'vue';

const currentYear = computed(() => new Date().getFullYear());
</script>

<style scoped>
.footer {
  background-color: #4F46E5; /* Couleur principale indigo */
  color: white;
  padding: 0;
  margin-top: 4rem;
}

.container {
  max-width: 1200px;
  margin: 0 auto;
  padding: 0 1.5rem;
}

.footer-content {
  display: grid;
  grid-template-columns: 1fr 2fr;
  gap: 3rem;
  padding: 4rem 0;
}

/* Section newsletter */
.newsletter-section {
  display: flex;
  flex-direction: column;
  gap: 1.5rem;
}

.newsletter-section h3 {
  font-size: 1.5rem;
  font-weight: 600;
  margin: 0;
}

.newsletter-section p {
  font-size: 1rem;
  opacity: 0.9;
  margin: 0;
}

.connect-button-container {
  margin-top: 1rem;
}

.connect-button {
  display: flex;
  align-items: center;
  justify-content: space-between;
  background-color: white;
  color: #4F46E5;
  padding: 0.75rem 1.5rem;
  border-radius: 30px;
  text-decoration: none;
  font-weight: 600;
  transition: all 0.3s ease;
  width: 100%;
  max-width: 250px;
  box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);
}

.email-input:focus {
  outline: none;
}

.subscribe-btn {
  background: transparent;
  border: none;
  color: white;
  font-size: 1.25rem;
  cursor: pointer;
  padding: 0 0.5rem;
}

.social-icons {
  display: flex;
  gap: 1.25rem;
  margin-top: 1rem;
}

.social-icons a {
  color: white;
  font-size: 1.25rem;
  transition: opacity 0.3s ease;
}

.social-icons a:hover {
  opacity: 0.8;
}

/* Section liens */
.footer-links {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 2rem;
}

.link-column h4 {
  font-size: 1rem;
  font-weight: 600;
  margin-bottom: 1.5rem;
  text-transform: uppercase;
  letter-spacing: 0.5px;
}

.link-column ul {
  list-style: none;
  padding: 0;
  margin: 0;
}

.link-column li {
  margin-bottom: 0.75rem;
}

.link-column a {
  color: rgba(255, 255, 255, 0.8);
  text-decoration: none;
  transition: color 0.3s ease;
  font-size: 0.95rem;
}

.link-column a:hover {
  color: white;
}

/* Barre de copyright */
.footer-bottom {
  background-color: rgba(0, 0, 0, 0.1);
  padding: 1.5rem 0;
}

.footer-bottom-content {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.logo-copyright {
  display: flex;
  align-items: center;
  gap: 1rem;
}

.footer-logo {
  height: 30px;
  width: auto;
}

.logo-copyright p {
  font-size: 0.9rem;
  opacity: 0.9;
  margin: 0;
}

.language-selector select {
  background-color: transparent;
  border: 1px solid rgba(255, 255, 255, 0.3);
  color: white;
  padding: 0.5rem 1rem;
  border-radius: 4px;
  font-size: 0.9rem;
  cursor: pointer;
}

.language-selector select:focus {
  outline: none;
  border-color: white;
}

.language-selector option {
  background-color: #4F46E5;
  color: white;
}

@media (max-width: 992px) {
  .footer-content {
    grid-template-columns: 1fr;
    gap: 3rem;
  }
  
  .newsletter-section {
    max-width: 500px;
  }
}

@media (max-width: 768px) {
  .footer-links {
    grid-template-columns: repeat(2, 1fr);
    gap: 2rem;
  }
  
  @media (max-width: 480px) {
    .footer-container {
      grid-template-columns: 1fr;
    }
  
    .footer-bottom {
      flex-direction: column;
      text-align: center;
      gap: 1rem;
    }
  }}
  </style>
