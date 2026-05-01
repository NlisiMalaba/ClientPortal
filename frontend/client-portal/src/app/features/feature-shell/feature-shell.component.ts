import { ChangeDetectionStrategy, Component, computed, inject } from '@angular/core';
import { ActivatedRoute } from '@angular/router';

@Component({
  selector: 'app-feature-shell',
  standalone: true,
  template: `
    <main class="min-h-screen p-6 bg-muted/30">
      <section class="max-w-5xl mx-auto rounded-lg border bg-card p-6 shadow-sm">
        <h1 class="text-2xl font-semibold mb-2">{{ title() }}</h1>
        <p class="text-sm text-muted-foreground">
          {{ description() }}
        </p>
      </section>
    </main>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class FeatureShellComponent {
  private readonly route = inject(ActivatedRoute);

  readonly title = computed(
    () => (this.route.snapshot.data['title'] as string | undefined) ?? 'Feature',
  );

  readonly description = computed(
    () =>
      (this.route.snapshot.data['description'] as string | undefined) ??
      'Feature module scaffolded and lazy-loaded.',
  );
}
