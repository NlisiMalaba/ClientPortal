import { Routes } from '@angular/router';

export const AUTH_ROUTES: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('./login-screen.component').then((m) => m.LoginScreenComponent),
    data: {
      title: 'Sign in',
      description: 'Email and password authentication for business portal users.',
    },
  },
  {
    path: 'register',
    loadComponent: () =>
      import('./register-screen.component').then((m) => m.RegisterScreenComponent),
    data: {
      title: 'Register business',
      description: 'Create a tenant and owner account for the business portal.',
    },
  },
  {
    path: 'accept-invitation',
    loadComponent: () =>
      import('./accept-invitation-screen.component').then(
        (m) => m.AcceptInvitationScreenComponent,
      ),
    data: {
      title: 'Accept invitation',
      description: 'Set account password from an invitation link.',
    },
  },
  {
    path: 'forgot-password',
    loadComponent: () =>
      import('../feature-shell/feature-shell.component').then(
        (m) => m.FeatureShellComponent,
      ),
    data: {
      title: 'Forgot Password',
      description: 'Password reset flow placeholder for business portal users.',
    },
  },
];
