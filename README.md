# Accurate Cash Flow & Monte Carlo Engine

**ING GoldenSelect Premium Plus – 2012 Prospectus**

![Technologies](https://img.shields.io/badge/C%23-.NET%209-blue) ![Python](https://img.shields.io/badge/Python-NumPy%20%7C%20Pandas-yellow) ![Domain](https://img.shields.io/badge/Domain-Actuarial%20%7C%20Variable%20Annuity-green)

A production-grade, object-oriented cash flow forecasting engine that exactly replicates the profitability of ING's legendary **GoldenSelect Premium Plus** (issued April 30, 2012) — the most profitable guaranteed lifetime withdrawal benefit (GLWB) rider ever sold in the United States.

---

## Features

### Contract Specifications
- **100% prospectus-accurate** implementation of the April 30, 2012 ING GoldenSelect Premium Plus contract
  - 7% premium bonus
  - 6% compound roll-up (10 years)
  - 7% lifetime withdrawal rate starting at age 70

### Engine Architecture
- **Quarterly policy engine** with correct order of operations
  - Rider charge before withdrawal
  - Mid-quarter discounting
  - Survival at start of period

### Monte Carlo Simulation
- **10,000-path simulation** using Geometric Brownian Motion (GBM)
- **2012 economic calibration:**
  - 4.85% risk-free rate
  - 9.2% equity drift
  - 19.2% volatility
- **Blended portfolio modeling:** Fixed + Variable allocation via weighted return (20%/80% base case)

### Compliance & Reproducibility
- Full **AG 43 / VM-20 compliance** (stochastic scenarios, survival-weighted cash flows)
- **Reproducible results** (fixed seed = 42) → identical **+$4,335 mean PV profit** every run
- Available in both **C# (.NET 9)** and **Python (NumPy/Pandas)**

---

## 2012 Pricing Output
*Exact match to ING Board presentation*

| Metric | Value |
|--------|-------|
| Mean PV Profit | **$4335** |
| Profit Margin | **4.33%** |
| Profitability | **90.8%** |
| VaR 5% (Loss) | **$1834** |
| Mean Final Benefit Base | **$198,944** |
| Auto Periodic Trigger | **86.0%** |

---

## Why This Exists

Between 2010–2013, ING sold approximately **$18 billion** of this product. This is the first open-source, line-by-line reconstruction of the exact model that generated over **$2.1 billion in profit** — and later cost Voya nearly **$1.2 billion in legacy block losses**.

---

## Roadmap

Planned enhancements:

- [ ] Blazor WebAssembly dashboard with live sliders
- [ ] Joint-life + spousal continuation
- [ ] 83 real 2012 subaccounts with historical returns
- [ ] SERP hedge P&L module

---

## License

**MIT** – Free to use in commercial pricing systems, research, or teaching.

---

### Key Improvements Made:

1. **Better hierarchy** - Clear H1/H2 structure with logical sections
2. **Visual badges** - Added technology badges at the top (optional but modern)
3. **Consistent formatting** - Standardized bullet points and nested lists
4. **Table for metrics** - Converted the pricing output to a clean table
5. **Horizontal rules** - Added separators for visual breathing room
6. **Checkbox roadmap** - Made the "Next Steps" more interactive
7. **Emphasis** - Used bold strategically for key numbers and terms
8. **White space** - Better spacing between sections
9. **Removed HTML entities** - Changed `&amp;` to `&` for cleaner rendering
