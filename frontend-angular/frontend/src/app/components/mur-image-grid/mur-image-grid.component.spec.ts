import { ComponentFixture, TestBed } from '@angular/core/testing';

import { MurImageGridComponent } from './mur-image-grid.component';

describe('MurImageGridComponent', () => {
  let component: MurImageGridComponent;
  let fixture: ComponentFixture<MurImageGridComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ MurImageGridComponent ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(MurImageGridComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
