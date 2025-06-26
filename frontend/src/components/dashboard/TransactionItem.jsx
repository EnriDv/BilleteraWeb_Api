import PropTypes from 'prop-types';
import { ArrowUpRight, ArrowDownLeft } from 'lucide-react';
import { formatCurrency } from '../../utils/formatters';import { useState } from 'react';
import { toast } from 'react-hot-toast';
import { UserServices } from '../../api/userServices.js';

export const TransactionItem = ({ transaction, currentWallet, onAccept }) => {
    const [isAccepting, setIsAccepting] = useState(false);
    const isSent = transaction.sourceWalletId === currentWallet?.id;
    const isReceived = transaction.destinationWalletId === currentWallet?.id;
    const status = transaction.status || transaction.Status; // backend puede retornar status o Status
    const formattedDate = new Date(transaction.timestamp).toLocaleDateString('es-ES', {
        day: '2-digit',
        month: 'short',
        year: 'numeric',
    });

    const statusBadge = (
        <span className={`ml-2 px-2 py-0.5 rounded text-xs font-bold 
            ${status === 'PENDING' ? 'bg-yellow-100 text-yellow-700' : 'bg-green-100 text-green-700'}`}
        >
            {status === 'PENDING' ? 'Pendiente' : 'Completada'}
        </span>
    );

    const showAcceptButton = (status === 'PENDING') && isReceived;

    // --- DEBUGGING LOG ---
    // Este log te ayudará a ver en la consola del navegador por qué el botón no aparece.
    // Busca una transacción pendiente que hayas recibido y revisa estos valores.
    if (status === 'PENDING') {
        console.log({
            component: 'TransactionItem',
            transactionId: transaction.id,
            status: status,
            shouldBePending: status === 'PENDING',
            destinationWalletId: transaction.destinationWalletId,
            typeOfDestinationWalletId: typeof transaction.destinationWalletId,
            currentWalletId: currentWallet?.id,
            typeOfCurrentWalletId: typeof currentWallet?.id,
            isReceived: isReceived,
            showAcceptButton: showAcceptButton,
        });
    }
    // --- END DEBUGGING LOG ---

    const handleAccept = async () => {
 
        console.log(transaction.status);
        setIsAccepting(true);
        try {
            await UserServices.acceptTransaction(transaction.id);
            toast.success('Transacción aceptada');
            if (onAccept) onAccept();
        } catch (error) {
            toast.error(error.response?.data?.message || 'No se pudo aceptar la transacción.');
        } finally {
            setIsAccepting(false);
        }
    };

    const description = transaction.description || `Transferencia ${isSent ? 'a' : 'de'} ${transaction.contactName || 'Desconocido'}`;

    return (
        <li className="flex items-center p-3 hover:bg-gray-50 rounded-md transition-colors">
            <div className={`flex-shrink-0 w-10 h-10 rounded-full flex items-center justify-center mr-4 ${isSent ? 'bg-red-100' : 'bg-green-100'}`}>
                {isSent 
                    ? <ArrowUpRight className="w-5 h-5 text-red-600" /> 
                    : <ArrowDownLeft className="w-5 h-5 text-green-600" />
                }
            </div>
            <div className="flex-1">
                <p className="font-semibold text-gray-800 flex items-center">
                    {description} {statusBadge}
                </p>
                <p className="text-sm text-gray-500">{formattedDate}</p>
            </div>
            <p className={`font-semibold text-lg ${isSent ? 'text-red-600' : 'text-green-600'}`}>
                {isSent ? '-' : '+'}
                {formatCurrency(transaction.amount)}
            </p>
            {showAcceptButton && (
                <button
                    onClick={handleAccept}
                    disabled={isAccepting}
                    className="ml-4 px-3 py-1 bg-indigo-600 text-white rounded hover:bg-indigo-700 disabled:bg-indigo-300"
                >
                    {isAccepting ? 'Aceptando...' : 'Aceptar'}
                </button>
            )}
        </li>
    );
};


TransactionItem.propTypes = {
    transaction: PropTypes.object.isRequired,
    currentWallet: PropTypes.object,
    onAccept: PropTypes.func,
};