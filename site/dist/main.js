'use strict';
const lessonNav = document.querySelector('.lesson-nav');
lessonNav?.querySelectorAll('a').forEach(link => {
  link.addEventListener('click', () => { lessonNav.open = false; });
});
document.addEventListener('click', event => {
  if (lessonNav && !lessonNav.contains(event.target)) lessonNav.open = false;
});
document.addEventListener('keydown', event => {
  if (event.key === 'Escape' && lessonNav?.open) {
    lessonNav.open = false;
    lessonNav.querySelector('summary').focus();
  }
});
const chapters = {
  whole: { image: '/assets/cargo-one-whole.png', alt: 'Front view of one full orange crate labeled 1 on a large truck at the Cargo Crew dock, above a zero-to-one ruler.', caption: 'One full crate, labeled 1: the whole that every fraction is measured against.' },
  quarters: { image: '/assets/cargo-quarters.png', alt: 'Four delivery vehicles each carry a crate labeled one-quarter.', caption: 'Four equal quarters fill the same whole container.' },
  equivalence: { image: '/assets/cargo-equivalence.png', alt: 'Cargo Crew chapter showing two quarter pieces against the half-container target.', caption: 'Two quarter pieces take up the same length as one half.' }
};
document.querySelectorAll('[data-chapter]').forEach(button => {
  button.addEventListener('click', () => {
    const chapter = chapters[button.dataset.chapter];
    if (!chapter) return;
    document.querySelectorAll('[data-chapter]').forEach(item => {
      const selected = item === button;
      item.classList.toggle('active', selected);
      item.setAttribute('aria-pressed', String(selected));
    });
    const image = document.getElementById('chapter-image');
    image.src = chapter.image;
    image.alt = chapter.alt;
    document.getElementById('chapter-caption').textContent = chapter.caption;
  });
});
