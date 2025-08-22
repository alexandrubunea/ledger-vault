# ledger-vault

### > Work in progress...
# Ledger Vault

A secure, cross-platform desktop application for personal financial management with blockchain-inspired integrity protection.

## Overview

Ledger Vault is a locally-stored personal ledger application that provides secure transaction tracking with cryptographic integrity verification. Built on Avalonia UI framework, it offers a modern, responsive interface across Windows, macOS, and Linux platforms.

## Key Features

### 🔐 **Security & Integrity**
- **Password-protected access** - Your data is protected behind a password wall - only authenticated users can view and manage transactions
- **Chain hashing** - Each transaction is cryptographically linked to the previous one using SHA-256 hashing
- **Digital signatures** - HMAC-SHA256 signatures ensure transaction authenticity and prevent tampering
- **Integrity verification** - Real-time detection of any chain breaks or data corruption
- **Receipt protection** - Attached files are hashed and verified for integrity

### 💰 **Transaction Management**
- **Income & Expense tracking** - Separate management of incoming and outgoing transactions
- **Rich metadata** - Counterparty, description, amount, tags, and timestamp for each transaction
- **Receipt attachments** - Support for PNG, JPG, JPEG, and PDF files
- **Transaction reversal** - Built-in support for correcting or reversing transactions
- **Tag system** - Categorize transactions with custom tags for better organization

### 📊 **Analytics & Reporting**
- **Weekly charts** - Visualize income, expenses, and cash flow over time
- **Tag analysis** - See your most popular income and expense categories
- **Real-time balance** - Always know your current financial position
- **Multi-currency support** - Support for 180+ world currencies

### 📤 **Export Capabilities**
- **Multiple formats** - Export to PDF, CSV, XML, JSON, and Excel (XLSX)
- **Comprehensive data** - Include all transaction details, verification status, and integrity checks
- **Professional reports** - Formatted exports suitable for accounting and tax purposes

### 🏗️ **Technical Architecture**
- **Cross-platform** - Built with Avalonia UI for consistent experience across operating systems
- **Modern .NET** - Built on .NET 9 with modern C# features
- **SQLite database** - Local storage with ACID compliance
- **MVVM pattern** - Clean separation of concerns using CommunityToolkit.Mvvm
- **Dependency injection** - Modular, maintainable code architecture

## How It Works

### Chain Hashing
Each transaction contains a hash of the previous transaction, creating an unbreakable chain. If any transaction is modified, the chain breaks and the application immediately detects where the integrity violation occurred.

### Digital Signatures
Every transaction is signed using HMAC-SHA256 with a protected key stored in your local system. This ensures that only you can create valid transactions and prevents unauthorized modifications.

### Receipt Protection
When you attach a receipt or invoice, the application:
1. Generates a cryptographic hash of the file
2. Stores the hash with the transaction
3. Verifies the file integrity every time it's accessed
4. Detects any file tampering or corruption

## Installation

### Prerequisites
- .NET 9.0 Runtime
- Windows 10+, macOS 10.15+, or Linux (with GTK3)

### Download
Download the latest release for your platform from the [Releases](https://github.com/alexandrubunea/ledger-vault/releases) page.

### Build from Source
```bash
git clone https://github.com/alexandrubunea/ledger-vault.git
cd ledger-vault
dotnet restore
dotnet build
dotnet run
```

## Usage

### First Time Setup
1. Launch the application
2. Set your master password
3. Configure your preferred currency
4. Start adding transactions

### Adding Transactions
1. Navigate to the Income or Payments section
2. Click "Add Transaction"
3. Fill in counterparty, description, amount, and tags
4. Optionally attach a receipt or invoice
5. Save the transaction

### Viewing Analytics
- **Home Dashboard** - Overview of balance and recent activity
- **Weekly Charts** - Toggle between income, expenses, and cash flow views
- **Tag Analysis** - See spending patterns by category

### Exporting Data
1. Go to the Export section
2. Select your preferred format (PDF, CSV, XML, JSON, or Excel)
3. Choose export location
4. Monitor progress and download your report

## Security Features

### Local Storage
- All data is stored locally on your device
- No cloud synchronization or external servers

### Cryptographic Protection
- SHA-256 hashing for transaction integrity
- HMAC-SHA256 signatures for authenticity
- Protected key storage using Windows Data Protection API
- Secure random number generation for cryptographic operations

### Integrity Verification
- Automatic verification of all transactions on startup
- Real-time detection of chain breaks
- Visual indicators for verification status
- Detailed reporting of any integrity issues

## Data Structure

### Transaction Fields
- **ID** - Unique identifier
- **Counterparty** - Who you transacted with
- **Description** - Transaction details
- **Amount** - Positive for income, negative for expenses
- **Tags** - Categorization labels
- **Receipt** - Optional file attachment
- **Timestamp** - When the transaction occurred
- **Hash** - Cryptographic hash of transaction data
- **Previous Hash** - Link to previous transaction
- **Signature** - Digital signature for authenticity
- **Reversal ID** - Reference to reversed transaction (if applicable)

### Database Schema
- **user_information** - User profile and settings
- **transactions** - All transaction records
- **reversed_transactions** - Transaction reversal relationships

## Future Enhancements

The application is designed for extensibility. Planned improvements include:
- Additional chart types and analytics
- Automated backup and synchronization
- Enhanced encryption for local storage
- Budget planning and forecasting
- Tax reporting features

## Contributing

We welcome contributions! Please see our [Contributing Guidelines](CONTRIBUTING.md) for details on:
- Code style and standards
- Testing requirements
- Pull request process
- Development setup

## License

This project is licensed under the [LICENSE](LICENSE) file included in the repository.

## Support

- **Issues** - Report bugs or request features via [GitHub Issues](https://github.com/alexandrubunea/ledger-vault/issues)
- **Discussions** - Join community discussions in [GitHub Discussions](https://github.com/alexandrubunea/ledger-vault/discussions)
- **Documentation** - Check the [Wiki](https://github.com/alexandrubunea/ledger-vault/wiki) for detailed guides

## Acknowledgments

- Built with [Avalonia UI](https://avaloniaui.net/) for cross-platform compatibility
- Charts powered by [LiveCharts](https://livecharts.dev/)
- PDF generation using [iText](https://itextpdf.com/)
- Excel export via [ClosedXML](https://github.com/ClosedXML/ClosedXML)

---

**Ledger Vault** - Your personal, secure financial ledger with cryptographic integrity protection.
