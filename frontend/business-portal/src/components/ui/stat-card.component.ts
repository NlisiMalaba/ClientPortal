import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';

import { cn } from '@/components/lib/utils';

type TrendDirection = 'up' | 'down' | 'neutral';

@Component({
  selector: 'ui-stat-card',
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <article [class]="classes()">
      <p class="text-sm text-muted-foreground">{{ label() }}</p>
      <p class="text-2xl font-semibold tracking-tight">{{ value() }}</p>

      @if (showTrend()) {
        <p [class]="trendClasses()">
          <span class="font-semibold">{{ trendPrefix() }}{{ trendValue() }}</span>
          @if (trendLabel() !== '') {
            <span class="ml-1">{{ trendLabel() }}</span>
          }
        </p>
      }
    </article>
  `,
})
export class StatCardComponent {
  readonly label = input.required<string>();
  readonly value = input.required<string | number>();
  readonly trendValue = input<string | number>('');
  readonly trendLabel = input('');
  readonly trendDirection = input<TrendDirection>('neutral');
  readonly class = input('');

  readonly showTrend = computed(() => `${this.trendValue()}`.trim() !== '');

  readonly classes = computed(() =>
    cn(
      'rounded-xl border bg-card p-4 text-card-foreground shadow-sm',
      this.class(),
    ),
  );

  trendPrefix(): string {
    if (this.trendDirection() === 'up') {
      return '+';
    }

    if (this.trendDirection() === 'down') {
      return '-';
    }

    return '';
  }

  trendClasses(): string {
    switch (this.trendDirection()) {
      case 'up':
        return 'mt-2 text-xs text-emerald-700';
      case 'down':
        return 'mt-2 text-xs text-red-700';
      default:
        return 'mt-2 text-xs text-muted-foreground';
    }
  }
}
