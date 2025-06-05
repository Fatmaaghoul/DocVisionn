import { createRouter, createWebHistory } from 'vue-router'
import Header from '@/components/Header.vue';
import About from '@/views/About/About.vue';
import Login from '@/views/Auth/Login.vue';
import Profile from '@/views/Auth/Profile.vue';
import Users from '@/views/Admin/Users.vue';
import Register from '@/views/Auth/Register.vue';
//import ListDocument from '@/views/Documents/ListDocument.vue';
import AdminLayout from '@/layouts/AdminLayout.vue';
import Dashboard from '@/views/Admin/Dashboard.vue';
import AdminProfile from '@/views/Admin/AdminProfile.vue';
import ResetPassword from '@/views/Auth/ResetPassword.vue';
import ForgotPassword from '@/views/Auth/ForgotPassword.vue';
import Home from '@/views/Home/Home.vue';
import ConfirmEmail from '@/views/Auth/ConfirmEmail.vue';
import Cookies from 'js-cookie'; // Import de js-cookie
import Documents from '@/views/Admin/Documents.vue';
import Document from '@/views/Documents/ContentDocument/Document.vue';
import ListDocument from '@/views/Documents/ListDocument/ListDocument.vue';
import MyDocuments from '@/views/Admin/MyDocuments.vue';
import ConsultDocument from '@/views/Admin/ConsultDocument.vue';
import Parametre from '@/views/Admin/Parametre.vue';
const router = createRouter({
  history: createWebHistory(),
  routes: [
    // Routes principales avec Header dans App.vue
    {
      path: '/',
      name: 'home',
      component: Home
    },
    {
      path: '/about',
      name: 'About',
      component: About
    },
 
    {
      path: '/document',
      name: 'Document',
      component: ListDocument,
      meta: { requiresAuth: true }
    },
    
    {
      path: '/document/content/:id',
      name: 'ContentDocument',
      component: Document,
      meta: { requiresAuth: true }
    },
    {
      path: '/profile',
      name: 'Profile',
      component: Profile,
      meta: { requiresAuth: true }
    },
    // Routes d'authentification
    {
      path: '/login',
      name: 'Login',
      component: Login
    },
    {
      path: '/register',
      name: 'Register',
      component: Register
    },
    { 
      path: '/reset-password',
      name: 'ResetPassword', 
      component: ResetPassword 
    },
    { 
      path: '/forgot-password', 
      name: 'ForgotPassword', 
      component: ForgotPassword
    },
    { 
      path: '/confirm-email', 
      name: 'ConfirmEmail', 
      component: ConfirmEmail
    },
    // Route admin
    {
      path: '/admin',
      component: AdminLayout,
      meta: { requiresAuth: true, requiresAdmin: true },
      children: [
        {
          path: '',
          redirect: '/admin/dashboard'
        },
        {
          path: 'dashboard',
          name: 'Dashboard',
          component: Dashboard
        },
        {
          path: 'users',
          name: 'AdminUsers',
          component: Users
        },
        {
          path: 'profile',
          name: 'AdminProfile',
          component: AdminProfile
        },
        {
          path: 'documents',
          name: 'Documents',
          component: Documents
        },
        {
          path: 'my-documents',
          name: 'MyDocuments',
          component: MyDocuments
        },
        {
          path: 'documents/:id',
          name: 'ConsultDocument',
          component: ConsultDocument,
          //props: true
        },
        {
          path: 'parametre',
          name: 'Parametre',
          component: Parametre,
        }
      ]
    }
  ]
});

// Navigation guard
router.beforeEach((to, from, next) => {
  const token = Cookies.get('token'); // Récupère le token depuis les cookies
  
  // Si l'utilisateur va vers login alors qu'il est déjà connecté
  if ((to.path === '/login' || to.path === '/') && token) {
    try {
      const tokenParts = token.split('.');
      const payload = JSON.parse(atob(tokenParts[1]));
      const userRole = payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'];
      
      if (userRole === 'Admin') {
        next('/admin/dashboard');
        return;
      }
    } catch (error) {
      console.error('Error checking admin role:', error);
    }
  }

  if (to.matched.some(record => record.meta.requiresAuth)) {
    if (!token) {
      next('/login');
      return;
    }

    if (to.matched.some(record => record.meta.requiresAdmin)) {
      try {
        const tokenParts = token.split('.');
        const payload = JSON.parse(atob(tokenParts[1]));
        const userRole = payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'];
        
        if (userRole !== 'Admin') {
          next('/document');
          return;
        }
      } catch (error) {
        console.error('Error checking admin role:', error);
        next('/login');
        return;
      }
    }
  }

  next();
});

export default router;
