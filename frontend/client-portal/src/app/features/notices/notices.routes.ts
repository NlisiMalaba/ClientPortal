import { Routes } from '@angular/router';

export const NOTICES_ROUTES: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('../feature-shell/feature-shell.component').then(
        (m) => m.FeatureShellComponent,
      ),
    data: {
      title: 'Notices',
      description: 'Company announcements and read/unread state handling.',
    },
  },
];
