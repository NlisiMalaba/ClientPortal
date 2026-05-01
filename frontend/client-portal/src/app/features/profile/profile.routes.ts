import { Routes } from '@angular/router';

export const PROFILE_ROUTES: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('../feature-shell/feature-shell.component').then(
        (m) => m.FeatureShellComponent,
      ),
    data: {
      title: 'Profile',
      description: 'Manage personal details, password, and notification settings.',
    },
  },
];
