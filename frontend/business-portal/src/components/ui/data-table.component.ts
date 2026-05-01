import { ChangeDetectionStrategy, Component, computed, input, signal } from '@angular/core';

export type DataTableRow = Record<string, unknown>;
export type DataTableSortDirection = 'asc' | 'desc';

export interface DataTableColumn {
  key: string;
  header: string;
  sortable?: boolean;
  filterable?: boolean;
  cell?: (row: DataTableRow) => string | number | null | undefined;
}

@Component({
  selector: 'ui-data-table',
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <section class="rounded-xl border bg-card text-card-foreground shadow-sm">
      <div class="overflow-x-auto">
        <table class="w-full min-w-[640px] text-sm">
          <thead class="bg-muted/40">
            <tr>
              @for (column of columns(); track column.key) {
                <th class="px-4 py-3 text-left font-semibold">
                  <div class="inline-flex items-center gap-2">
                    <span>{{ column.header }}</span>
                    @if (column.sortable) {
                      <button
                        type="button"
                        class="rounded border border-input bg-background px-2 py-0.5 text-xs hover:bg-muted"
                        (click)="toggleSort(column.key)"
                      >
                        {{ sortIndicator(column.key) }}
                      </button>
                    }
                  </div>
                </th>
              }
            </tr>
            @if (hasFilterableColumns()) {
              <tr>
                @for (column of columns(); track column.key) {
                  <th class="px-4 pb-3">
                    @if (column.filterable) {
                      <input
                        type="text"
                        class="h-8 w-full rounded-md border border-input bg-background px-2 text-xs"
                        [value]="getFilterValue(column.key)"
                        [placeholder]="'Filter ' + column.header"
                        (input)="setFilter(column.key, $event)"
                      />
                    }
                  </th>
                }
              </tr>
            }
          </thead>

          <tbody>
            @if (loading()) {
              @for (rowIndex of skeletonRows(); track rowIndex) {
                <tr class="border-t">
                  @for (column of columns(); track column.key) {
                    <td class="px-4 py-3">
                      <div class="h-4 animate-pulse rounded bg-muted"></div>
                    </td>
                  }
                </tr>
              }
            } @else if (pagedRows().length === 0) {
              <tr class="border-t">
                <td class="px-4 py-8 text-center text-muted-foreground" [attr.colspan]="columns().length">
                  {{ emptyStateMessage() }}
                </td>
              </tr>
            } @else {
              @for (row of pagedRows(); track trackRow(row, $index)) {
                <tr class="border-t">
                  @for (column of columns(); track column.key) {
                    <td class="px-4 py-3">
                      {{ resolveCellValue(row, column) }}
                    </td>
                  }
                </tr>
              }
            }
          </tbody>
        </table>
      </div>

      <footer class="flex items-center justify-between gap-4 border-t px-4 py-3 text-sm">
        <div class="text-muted-foreground">
          Showing {{ rangeStart() }}-{{ rangeEnd() }} of {{ totalFilteredItems() }}
        </div>
        <div class="flex items-center gap-2">
          <label class="text-xs text-muted-foreground">Rows</label>
          <select
            class="h-8 rounded-md border border-input bg-background px-2"
            [value]="pageSize()"
            (change)="setPageSize($event)"
          >
            @for (size of pageSizeOptions(); track size) {
              <option [value]="size">{{ size }}</option>
            }
          </select>

          <button
            type="button"
            class="rounded-md border border-input bg-background px-3 py-1 hover:bg-muted disabled:opacity-50"
            [disabled]="boundedCurrentPage() <= 1"
            (click)="goToPage(boundedCurrentPage() - 1)"
          >
            Prev
          </button>
          <span class="min-w-20 text-center text-muted-foreground">
            Page {{ boundedCurrentPage() }} / {{ totalPages() }}
          </span>
          <button
            type="button"
            class="rounded-md border border-input bg-background px-3 py-1 hover:bg-muted disabled:opacity-50"
            [disabled]="boundedCurrentPage() >= totalPages()"
            (click)="goToPage(boundedCurrentPage() + 1)"
          >
            Next
          </button>
        </div>
      </footer>
    </section>
  `,
})
export class DataTableComponent {
  readonly columns = input.required<ReadonlyArray<DataTableColumn>>();
  readonly rows = input<ReadonlyArray<DataTableRow>>([]);
  readonly loading = input(false);
  readonly emptyStateMessage = input('No records found.');
  readonly rowTrackByKey = input('id');
  readonly pageSizeOptions = input<ReadonlyArray<number>>([10, 20, 50]);
  readonly defaultPageSize = input(10);

  readonly currentPage = signal(1);
  readonly pageSize = signal(10);
  readonly sortKey = signal<string | null>(null);
  readonly sortDirection = signal<DataTableSortDirection>('asc');
  readonly filters = signal<Record<string, string>>({});

  readonly hasFilterableColumns = computed(() =>
    this.columns().some((column) => column.filterable === true),
  );

  readonly filteredRows = computed(() => {
    const activeFilters = this.filters();
    const sourceRows = [...this.rows()];

    return sourceRows.filter((row) =>
      this.columns().every((column) => {
        if (!column.filterable) {
          return true;
        }

        const filterValue = (activeFilters[column.key] ?? '').trim().toLowerCase();
        if (filterValue === '') {
          return true;
        }

        const renderedValue = this.resolveCellValue(row, column).toLowerCase();
        return renderedValue.includes(filterValue);
      }),
    );
  });

  readonly sortedRows = computed(() => {
    const key = this.sortKey();
    if (key === null) {
      return this.filteredRows();
    }

    const direction = this.sortDirection();
    const factor = direction === 'asc' ? 1 : -1;
    const sortableColumn = this.columns().find((column) => column.key === key);
    if (sortableColumn === undefined) {
      return this.filteredRows();
    }

    return [...this.filteredRows()].sort((left, right) => {
      const leftValue = this.resolveCellValue(left, sortableColumn);
      const rightValue = this.resolveCellValue(right, sortableColumn);
      return leftValue.localeCompare(rightValue, undefined, { numeric: true }) * factor;
    });
  });

  readonly totalFilteredItems = computed(() => this.sortedRows().length);
  readonly totalPages = computed(() => {
    const pages = Math.ceil(this.totalFilteredItems() / this.effectivePageSize());
    return Math.max(1, pages);
  });

  readonly boundedCurrentPage = computed(() =>
    Math.max(1, Math.min(this.currentPage(), this.totalPages())),
  );

  readonly pagedRows = computed(() => {
    const page = this.boundedCurrentPage();
    const start = (page - 1) * this.effectivePageSize();
    const end = start + this.effectivePageSize();
    return this.sortedRows().slice(start, end);
  });

  readonly skeletonRows = computed(() =>
    Array.from({ length: Math.max(3, Math.min(this.effectivePageSize(), 8)) }, (_, index) => index),
  );

  readonly rangeStart = computed(() =>
    this.totalFilteredItems() === 0
      ? 0
      : (this.boundedCurrentPage() - 1) * this.effectivePageSize() + 1,
  );

  readonly rangeEnd = computed(() =>
    Math.min(
      this.boundedCurrentPage() * this.effectivePageSize(),
      this.totalFilteredItems(),
    ),
  );

  toggleSort(columnKey: string): void {
    if (this.sortKey() !== columnKey) {
      this.sortKey.set(columnKey);
      this.sortDirection.set('asc');
      return;
    }

    this.sortDirection.set(this.sortDirection() === 'asc' ? 'desc' : 'asc');
  }

  sortIndicator(columnKey: string): string {
    if (this.sortKey() !== columnKey) {
      return 'Sort';
    }

    return this.sortDirection() === 'asc' ? 'Asc' : 'Desc';
  }

  setFilter(columnKey: string, event: Event): void {
    const target = event.target as HTMLInputElement;
    const value = target.value ?? '';

    this.filters.update((existing) => ({
      ...existing,
      [columnKey]: value,
    }));
    this.currentPage.set(1);
  }

  getFilterValue(columnKey: string): string {
    return this.filters()[columnKey] ?? '';
  }

  setPageSize(event: Event): void {
    const target = event.target as HTMLSelectElement;
    const parsed = Number(target.value);
    const isValidOption = this.pageSizeOptions().includes(parsed);
    this.pageSize.set(isValidOption ? parsed : this.defaultPageSize());
    this.currentPage.set(1);
  }

  goToPage(page: number): void {
    const boundedPage = Math.max(1, Math.min(page, this.totalPages()));
    this.currentPage.set(boundedPage);
  }

  trackRow(row: DataTableRow, index: number): string | number {
    const trackByKey = this.rowTrackByKey();
    const maybeKey = row[trackByKey];
    if (typeof maybeKey === 'string' || typeof maybeKey === 'number') {
      return maybeKey;
    }

    return index;
  }

  resolveCellValue(row: DataTableRow, column: DataTableColumn): string {
    const value = column.cell?.(row) ?? row[column.key];
    if (value === null || value === undefined) {
      return '';
    }

    return String(value);
  }

  private effectivePageSize(): number {
    const value = this.pageSize();
    return Number.isFinite(value) && value > 0 ? value : this.defaultPageSize();
  }
}
