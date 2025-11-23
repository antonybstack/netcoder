import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: '',
    pathMatch: 'full',
    redirectTo: 'code',
  },
  {
    path: 'code',
    loadComponent: () => import('./components/code-page/code-page').then((m) => m.CodePage),
  },
  {
    path: 'sample',
    loadComponent: () => import('./components/sample/sample').then((m) => m.Sample),
  },
];
