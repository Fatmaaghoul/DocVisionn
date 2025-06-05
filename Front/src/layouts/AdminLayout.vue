<template>
  <div class="admin-layout">
    <!-- Sidebar -->
    <div class="sidebar" :class="{ 'collapsed': isSidebarCollapsed }">
      <div class="sidebar-header">
        <div class="logo-container">
          <i class="bi bi-grid-3x3-gap-fill logo-icon"></i>
          <span v-if="!isSidebarCollapsed" class="logo-text">Panneau d'administration
          </span>
        </div>
        <button class="collapse-btn" @click="toggleSidebar">
          <i class="bi" :class="isSidebarCollapsed ? 'bi-chevron-right' : 'bi-chevron-left'"></i>
        </button>
      </div>

      <div class="user-info" v-if="!isSidebarCollapsed" @click="goToProfile">
        <div class="avatar">
          <i class="bi bi-person-circle"></i>
        </div>
        <div class="user-details">
          <h6 class="username">{{ user?.email }}</h6>
          <span class="user-role">{{ user?.role }}</span>
        </div>
      </div>

      <nav class="sidebar-nav">
        <router-link to="/admin/dashboard" class="nav-item" :class="{ 'collapsed': isSidebarCollapsed }">
          <i class="bi bi-house-door"></i>
          <span v-if="!isSidebarCollapsed">Tableau de bord
          </span>
        </router-link>
        <div class="nav-section" v-if="!isSidebarCollapsed">
          <h6 class="nav-section-title">Interface</h6>
        </div>


        <router-link to="/admin/users" class="nav-item" :class="{ 'collapsed': isSidebarCollapsed }">
          <i class="bi bi-people"></i>
          <span v-if="!isSidebarCollapsed">Utilisateurs</span>
        </router-link>


        <router-link to="/admin/documents" class="nav-item" :class="{ 'collapsed': isSidebarCollapsed }">
          <i class="bi bi-file-earmark-text"></i>
          <span v-if="!isSidebarCollapsed">Documents</span>
        </router-link>
        <router-link to="/admin/my-documents" class="nav-item" :class="{ 'collapsed': isSidebarCollapsed }">
          <i class="bi bi-folder"></i>
          <span v-if="!isSidebarCollapsed">Mes Documents</span>
        </router-link>

        <router-link to="/admin/parametre" class="nav-item" :class="{ 'collapsed': isSidebarCollapsed }">
          <i class="bi bi-boxes"></i>
          <span v-if="!isSidebarCollapsed">Modéles</span>
        </router-link>
      </nav>

      <div class="sidebar-footer" v-if="!isSidebarCollapsed">
        <button class="btn-logout" @click="logout">
          <i class="bi bi-box-arrow-right"></i>
          <span>Déconnexion</span>
        </button>
      </div>
    </div>

    <!-- Main Content -->
    <div class="main-content" :class="{ 'expanded': isSidebarCollapsed }">
      <div class="content-wrapper">
        <router-view></router-view>
      </div>
    </div>
  </div>
</template>

<script>
import { ref, computed } from 'vue';
import { useRouter } from 'vue-router';
import { useStore } from 'vuex';
import { useDocumentStore } from '@/Store/analysis';
export default {
  name: 'AdminLayout',
  setup() {
    const router = useRouter();
    const store = useStore();
    const documentStore = useDocumentStore();
    const isSidebarCollapsed = ref(false);
    const user = computed(() => store.state.user);

    const logout = async () => {
      documentStore.stopAnalysing();
      localStorage.removeItem('isAnalysing');
      localStorage.removeItem('isExtracting');
      localStorage.removeItem('analysisTimer');
      localStorage.removeItem('progress');
      localStorage.removeItem('message');
      localStorage.removeItem('taskType');
      localStorage.removeItem('taskItemName');
      await store.dispatch('logout');
      router.push('/login');
    };

    const toggleSidebar = () => {
      isSidebarCollapsed.value = !isSidebarCollapsed.value;
    };
    
    const goToProfile = () => {
      router.push('/admin/profile');
    };

    return {
      isSidebarCollapsed,
      toggleSidebar,
      user,
      goToProfile,
      logout
    };
  }
};
</script>

<style scoped>
.admin-layout {
  display: flex;
  min-height: 100vh;
  background-color: #f5f7fa;
}

/* Sidebar Modern Style */
.sidebar {
  width: 280px;
  background: linear-gradient(135deg, #2c3e50 0%, #1a2533 100%);
  display: flex;
  flex-direction: column;
  transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
  position: fixed;
  height: 100vh;
  z-index: 1000;
  box-shadow: 4px 0 20px rgba(0, 0, 0, 0.1);
  color: white;
}

.sidebar.collapsed {
  width: 80px;
}

.sidebar-header {
  padding: 1.5rem 1.25rem;
  display: flex;
  align-items: center;
  justify-content: space-between;
  border-bottom: 1px solid rgba(255, 255, 255, 0.1);
}

.logo-container {
  display: flex;
  align-items: center;
  gap: 12px;
}

.logo-icon {
  font-size: 1.8rem;
  color: #4F46E5;
}

.logo-text {
  font-weight: 600;
  font-size: 1.2rem;
  color: white;
}

.collapse-btn {
  border: none;
  background: rgba(255, 255, 255, 0.1);
  color: white;
  cursor: pointer;
  width: 32px;
  height: 32px;
  border-radius: 8px;
  display: flex;
  align-items: center;
  justify-content: center;
  transition: all 0.2s ease;
}

.collapse-btn:hover {
  background: rgba(255, 255, 255, 0.2);
}

.user-info {
  padding: 1.5rem 1.25rem;
  display: flex;
  align-items: center;
  gap: 1rem;
  border-bottom: 1px solid rgba(255, 255, 255, 0.1);
  cursor: pointer;
  transition: background 0.2s ease;
}

.user-info:hover {
  background: rgba(255, 255, 255, 0.05);
}

.avatar {
  width: 40px;
  height: 40px;
  border-radius: 50%;
  background: rgba(255, 255, 255, 0.1);
  display: flex;
  align-items: center;
  justify-content: center;
}

.avatar i {
  font-size: 1.5rem;
  color: white;
}

.user-details {
  display: flex;
  flex-direction: column;
}

.username {
  margin: 0;
  font-size: 0.95rem;
  font-weight: 500;
  color: white;
}

.user-role {
  font-size: 0.75rem;
  color: rgba(255, 255, 255, 0.7);
}

.sidebar-nav {
  padding: 0.5rem 0;
  flex-grow: 1;
  overflow-y: auto;
}

.nav-section {
  padding: 1rem 1.5rem 0.5rem;
}

.nav-section-title {
  color: rgba(255, 255, 255, 0.5);
  font-size: 0.75rem;
  text-transform: uppercase;
  letter-spacing: 1px;
  font-weight: 600;
}

.nav-item {
  display: flex;
  align-items: center;
  padding: 0.75rem 1.5rem;
  color: rgba(255, 255, 255, 0.8);
  text-decoration: none;
  transition: all 0.2s ease;
  gap: 1rem;
  margin: 0.25rem 0.75rem;
  border-radius: 8px;
}

.nav-item:hover {
  color: white;
  background: rgba(255, 255, 255, 0.1);
}

.nav-item.router-link-active {
  color: white;
  background:#4F46E5 ;
  box-shadow: 0 4px 6px rgba(79, 209, 197, 0.2);
}

.nav-item.collapsed {
  padding: 0.75rem;
  justify-content: center;
  margin: 0.25rem;
}

.nav-item i {
  font-size: 1.2rem;
}

.sidebar-footer {
  padding: 1rem 1.25rem;
  border-top: 1px solid rgba(255, 255, 255, 0.1);
}

.btn-logout {
  width: 100%;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 0.5rem;
  color: rgba(255, 255, 255, 0.8);
  background: rgba(255, 255, 255, 0.1);
  border: none;
  padding: 0.65rem 1rem;
  border-radius: 8px;
  transition: all 0.2s ease;
  font-weight: 500;
}

.btn-logout:hover {
  background: rgba(255, 255, 255, 0.2);
  color: white;
}

/* Main Content Modern Style */
.main-content {
  flex-grow: 1;
  background: #f5f7fa;
  transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
  margin-left: 280px;
  min-height: 100vh;
}

.main-content.expanded {
  margin-left: 80px;
}

.content-wrapper {
  padding: 2rem;
  min-height: calc(100vh - 70px);
}

/* Modern Scrollbar */
.sidebar-nav::-webkit-scrollbar {
  width: 6px;
}

.sidebar-nav::-webkit-scrollbar-track {
  background: rgba(255, 255, 255, 0.05);
}

.sidebar-nav::-webkit-scrollbar-thumb {
  background: rgba(255, 255, 255, 0.2);
  border-radius: 3px;
}

.sidebar-nav::-webkit-scrollbar-thumb:hover {
  background: rgba(255, 255, 255, 0.3);
}

/* Responsive adjustments */
@media (max-width: 768px) {
  .sidebar {
    transform: translateX(-100%);
  }
  
  .sidebar.collapsed {
    transform: translateX(0);
    width: 70px;
  }
  
  .main-content {
    margin-left: 0;
  }
  
  .main-content.expanded {
    margin-left: 70px;
  }
}
</style>
