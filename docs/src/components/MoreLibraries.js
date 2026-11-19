import React from 'react';
import clsx from 'clsx';
import useBaseUrl from '@docusaurus/useBaseUrl';
import styles from './MoreLibraries.module.css';

// Keep in step with BlazorBootstrap.Demo.RCL/Components/Layout/MoreLibrariesSection.razor.cs.
const libraries = [
  {
    name: 'UnlimitedPDF',
    description: 'A pure .NET library to create, merge and split PDFs, and convert HTML to PDF.',
    logo: 'img/libraries/unlimitedpdf.png',
    website: 'https://unlimitedpdf.dev/',
    github: 'https://github.com/unlimited-pdf/UnlimitedPDF',
    nuget: 'https://www.nuget.org/packages/UnlimitedPDF',
  },
  {
    name: 'BlazorExpress.ChartJS',
    description: 'Chart.js charts for Blazor, configured in C#.',
    logo: 'img/libraries/blazorexpress-chartjs.svg',
    website: 'https://chartjs.blazorexpress.com/',
    github: 'https://github.com/BlazorExpress/BlazorExpress.ChartJS',
    nuget: 'https://www.nuget.org/packages/BlazorExpress.ChartJS',
  },
  {
    name: 'BlazorExpress.Bulma',
    description: 'Enterprise-class Blazor components built on the Bulma CSS framework.',
    logo: 'img/libraries/blazorexpress-bulma.svg',
    website: 'https://bulma.blazorexpress.com/',
    github: 'https://github.com/BlazorExpress/BlazorExpress.Bulma',
    nuget: 'https://www.nuget.org/packages/BlazorExpress.Bulma',
  },
];

function Action({href, label, library, primary}) {
  return (
    <a
      className={clsx('button button--sm button--outline', primary ? 'button--primary' : 'button--secondary', styles.action)}
      href={href}
      target="_blank"
      rel="noopener noreferrer">
      {label}
      <span className={styles.visuallyHidden}> for {library}</span>
    </a>
  );
}

function LibraryCard({library}) {
  return (
    <div className={clsx('col col--4', styles.col)}>
      <div className={clsx('card', styles.card)}>
        <div className={clsx('card__body', styles.body)}>
          <img className={styles.logo} src={useBaseUrl(library.logo)} width="48" height="48" alt="" />
          <h3 className={styles.name}>{library.name}</h3>
          <p className={styles.description}>{library.description}</p>
          <div className={styles.actions}>
            <Action href={library.website} label="Website" library={library.name} primary />
            <Action href={library.github} label="GitHub" library={library.name} />
            <Action href={library.nuget} label="NuGet" library={library.name} />
          </div>
        </div>
      </div>
    </div>
  );
}

export default function MoreLibraries() {
  return (
    <section className={styles.section} aria-labelledby="bd-more-libraries-heading">
      <div className="container">
        <h2 id="bd-more-libraries-heading" className={styles.heading}>More libraries from the team</h2>
        <p className={styles.intro}>Free, open-source .NET libraries built by the same team.</p>
        <div className="row">
          {libraries.map((library) => (
            <LibraryCard key={library.name} library={library} />
          ))}
        </div>
      </div>
    </section>
  );
}
