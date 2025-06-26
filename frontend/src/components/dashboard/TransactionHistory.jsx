import PropTypes from 'prop-types';
import { TransactionItem } from './TransactionItem';


export const TransactionHistory = ({ isLoading, error, transactions, currentWallet }) => {
    const renderContent = () => {
        if (isLoading) return;
        if (error) { console.log(error); }
        if (transactions.length === 0) { return <p className="text-sm text-gray-500 text-center py-4">No hay transacciones recientes.</p>;}
        
        if (!Array.isArray(transactions) || transactions.length === 0) {
            return <p className="text-sm text-gray-500 text-center py-4">No hay transacciones recientes.</p>;
        }
        return (
            <ul className="space-y-3">
                {transactions.map(tx => (
                    <TransactionItem key={tx.id} transaction={tx} currentWallet={currentWallet} />
                ))}
            </ul>
        );
    };

    return (
        <div className="w-full max-w-lg mx-auto mt-8 p-6 bg-white rounded-xl shadow-md">
            <h2 className="text-lg font-bold mb-4">Actividad Reciente</h2>
            {renderContent()}
        </div>
    );
};

TransactionHistory.propTypes = {
    isLoading: PropTypes.bool.isRequired,
    error: PropTypes.object,
    transactions: PropTypes.array.isRequired,
    onRetry: PropTypes.func.isRequired,
    currentWallet: PropTypes.object,
};