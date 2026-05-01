import { Routes } from '@angular/router';

export const REQUESTS_ROUTES: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('../feature-shell/feature-shell.component').then(
        (m) => m.FeatureShellComponent,
      ),
    data: {
      title: 'Requests',
      description: 'Submit requests and track priority and current status.',
    },
  },
];
