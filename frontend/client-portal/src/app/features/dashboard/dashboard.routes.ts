import { Routes } from '@angular/router';

export const DASHBOARD_ROUTES: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('../feature-shell/feature-shell.component').then(
        (m) => m.FeatureShellComponent,
      ),
    data: {
      title: 'Client Dashboard',
      description:
        'Active projects, outstanding invoices, upcoming meetings, and messages.',
    },
  },
];
