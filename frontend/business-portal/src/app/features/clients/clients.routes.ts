import { Routes } from '@angular/router';

export const CLIENTS_ROUTES: Routes = [
  {
    path: 'invite-onboarding',
    loadComponent: () =>
      import('./client-invite-onboarding.component').then(
        (m) => m.ClientInviteOnboardingComponent,
      ),
    data: {
      title: 'Invite & Onboarding',
      description: 'Invite clients and track onboarding completion.',
    },
  },
  {
    path: '',
    loadComponent: () =>
      import('./clients-list.component').then((m) => m.ClientsListComponent),
    data: {
      title: 'Clients',
      description: 'Client list, onboarding, and detailed client profile views.',
    },
  },
  {
    path: ':clientId',
    loadComponent: () =>
      import('./client-detail.component').then((m) => m.ClientDetailComponent),
    data: {
      title: 'Client Detail',
      description: 'Overview, projects, invoices, documents, messages, and requests.',
    },
  },
];
